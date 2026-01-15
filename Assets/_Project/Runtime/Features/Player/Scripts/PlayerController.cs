using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerHealth))]
public sealed class PlayerController : Singleton<PlayerController, SceneScope>
{
    [SerializeField] private PlayerSettings settings;
    [SerializeField] private AimCursor aimCursor;

    [Header("Facing")]
    [SerializeField] private float facingDeadZoneX = 0.05f;
    [SerializeField] private float facingMoveThreshold = 0.1f;

    private PlayerMotor motor;
    private PlayerCombat combat;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private float attackTimer;
    private bool attacking;
    private int lockedFacingDir;

    private QueuedAction queuedAction;

    public int FacingDir { get; private set; } = 1;
    public bool IsAttacking => attacking;

    protected override void SingletonAwake()
    {
        motor = GetComponent<PlayerMotor>();
        combat = GetComponent<PlayerCombat>();

        motor.SetSettings(settings);
    }

    private void Update()
    {
        Vector2 move = InputManager.Instance.MoveVector;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer < 0f)
                attackTimer = 0f;
        }

        attacking = attackTimer > 0f;

        bool runHeld = InputManager.Instance.RunHeld;
        if (attacking)
            runHeld = false;

        bool isRunning = runHeld && Mathf.Abs(move.x) > facingMoveThreshold;
        bool isDashing = motor.IsDashing;

        Vector3 aimDir = aimCursor.GetAimDir(transform.position);

        if (attacking)
        {
            FacingDir = lockedFacingDir;
        }
        else if (isDashing)
        {
            FacingDir = motor.DashDir;
        }
        else if (isRunning)
        {
            FacingDir = move.x >= 0f ? 1 : -1;
        }
        else
        {
            float dx = aimCursor.WorldPosition.x - transform.position.x;

            if (Mathf.Abs(dx) > facingDeadZoneX)
                FacingDir = dx > 0f ? 1 : -1;
            else if (Mathf.Abs(move.x) > facingMoveThreshold)
                FacingDir = move.x > 0f ? 1 : -1;
        }

        motor.SetFacingDir(FacingDir);

        motor.SetJumpHeld(InputManager.Instance.JumpHeld);

        bool locked = motor.IsDashing || motor.IsLedgeAssisting || attacking;

        if (!attacking && InputManager.Instance.DashDown)
        {
            if (locked)
                queuedAction = QueuedAction.Dash;
            else
                motor.TryDash(settings, GetDashDirX(move.x));
        }

        if (InputManager.Instance.AttackDown)
        {
            if (locked)
                queuedAction = QueuedAction.Attack;
            else
                StartAttack(aimDir, false);
        }

        if (InputManager.Instance.SkillDown)
        {
            if (locked)
                queuedAction = QueuedAction.Skill;
            else
                StartAttack(aimDir, true);
        }

        if (InputManager.Instance.DiceSkillDown)
        {
            if (locked)
                queuedAction = QueuedAction.DiceSkill;
            else
                StartDiceSkill();
        }

        if (!locked && queuedAction != QueuedAction.None)
            ConsumeQueued(aimDir, move.x);

        float speed = settings.moveSpeed;

        if (!attacking && runHeld)
            speed *= settings.runMultiplier;

        bool backwalking = IsBackwalking(move.x, FacingDir);

        if (attacking)
        {
            if (backwalking)
                speed *= settings.attackBackwalkMultiplier;
            else
                speed *= settings.attackMoveMultiplier;
        }
        else
        {
            if (backwalking)
                speed *= settings.backwalkMultiplier;
        }

        motor.SetMove(move.x, speed, settings.accelTimeToMax);

        UpdateJump();

        motor.ClampFallSpeed(settings.maxFallSpeed);
        motor.TryStartLedgeAssist(settings, FacingDir);
    }

    private void StartAttack(Vector3 aimDir, bool isSkill)
    {
        combat.BeginAttack(settings, aimDir, isSkill);

        attackTimer = isSkill ? settings.skillAnimTime : settings.attackAnimTime;
        lockedFacingDir = FacingDir;

        queuedAction = QueuedAction.None;
    }

    private void StartDiceSkill()
    {
        attackTimer = 0.2f;
        lockedFacingDir = FacingDir;

        queuedAction = QueuedAction.None;
    }

    private void ConsumeQueued(Vector3 aimDir, float moveAxis)
    {
        QueuedAction q = queuedAction;
        queuedAction = QueuedAction.None;

        if (q == QueuedAction.Attack)
        {
            StartAttack(aimDir, false);
            return;
        }

        if (q == QueuedAction.Skill)
        {
            StartAttack(aimDir, true);
            return;
        }

        if (q == QueuedAction.Dash)
        {
            if (!attacking)
                motor.TryDash(settings, GetDashDirX(moveAxis));
            return;
        }

        if (q == QueuedAction.DiceSkill)
            StartDiceSkill();
    }

    private void UpdateJump()
    {
        if (motor.IsGrounded)
            coyoteTimer = settings.coyoteTime;
        else
        {
            coyoteTimer -= Time.deltaTime;
            if (coyoteTimer < 0f)
                coyoteTimer = 0f;
        }

        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
            if (jumpBufferTimer < 0f)
                jumpBufferTimer = 0f;
        }

        if (InputManager.Instance.JumpDown)
            jumpBufferTimer = settings.jumpBufferTime;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            motor.Jump();
        }

        if (InputManager.Instance.JumpUp)
            motor.CutJump(settings.jumpCutMultiplier);
    }

    private bool IsBackwalking(float moveAxis, int facingDir)
    {
        if (Mathf.Abs(moveAxis) < 0.001f)
            return false;

        int moveDir = moveAxis >= 0f ? 1 : -1;
        return moveDir != facingDir;
    }

    private float GetDashDirX(float moveAxis)
    {
        if (Mathf.Abs(moveAxis) < 0.001f)
            return FacingDir;

        return moveAxis >= 0f ? 1f : -1f;
    }

    private enum QueuedAction
    {
        None,
        Attack,
        Skill,
        DiceSkill,
        Dash
    }
}