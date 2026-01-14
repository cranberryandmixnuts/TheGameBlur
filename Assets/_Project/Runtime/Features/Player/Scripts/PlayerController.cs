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

    private float actionTimer;
    private QueuedAction queuedAction;
    private bool currentActionMovementPenalty;

    public int FacingDir { get; private set; } = 1;

    protected override void SingletonAwake()
    {
        motor = GetComponent<PlayerMotor>();
        combat = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        Vector2 move = InputManager.Instance.MoveVector;

        float dx = aimCursor.WorldPosition.x - transform.position.x;

        if (Mathf.Abs(dx) > facingDeadZoneX)
            FacingDir = dx > 0f ? 1 : -1;
        else if (Mathf.Abs(move.x) > facingMoveThreshold)
            FacingDir = move.x > 0f ? 1 : -1;

        motor.SetFacingDir(FacingDir);

        Vector3 aimDir = aimCursor.GetAimDir(transform.position);

        if (actionTimer > 0f)
        {
            actionTimer -= Time.deltaTime;
            if (actionTimer < 0f)
                actionTimer = 0f;
        }

        bool locked = actionTimer > 0f || motor.IsDashing || motor.IsLedgeAssisting;

        if (InputManager.Instance.DashDown)
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

        bool runHeld = InputManager.Instance.RunHeld;

        if (InputManager.Instance.AttackDown || InputManager.Instance.SkillDown)
            runHeld = false;

        float speed = settings.moveSpeed;

        if (runHeld)
            speed *= settings.runMultiplier;
        else if (IsBackwalking(move.x, aimDir.x))
            speed *= settings.backwalkMultiplier;

        if (currentActionMovementPenalty && actionTimer > 0f)
            speed *= settings.attackMoveMultiplier;

        motor.SetMove(move.x, speed, settings.accelTimeToMax);

        UpdateJump();

        motor.ClampFallSpeed(settings.maxFallSpeed);
        motor.TryStartLedgeAssist(settings, FacingDir);
    }

    private void StartAttack(Vector3 aimDir, bool isSkill)
    {
        combat.BeginAttack(settings, aimDir, isSkill);

        actionTimer = isSkill ? settings.skillDuration : settings.attackDuration;
        currentActionMovementPenalty = true;

        queuedAction = QueuedAction.None;
    }

    private void StartDiceSkill()
    {
        actionTimer = 0.2f;
        currentActionMovementPenalty = true;

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

            motor.Jump(settings.jumpVelocity);
        }

        if (InputManager.Instance.JumpUp)
            motor.CutJump(settings.jumpCutMultiplier);
    }

    private bool IsBackwalking(float moveAxis, float aimX)
    {
        if (Mathf.Abs(moveAxis) < 0.001f)
            return false;

        int moveDir = moveAxis >= 0f ? 1 : -1;
        int aimDir = aimX >= 0f ? 1 : -1;

        return moveDir != aimDir;
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