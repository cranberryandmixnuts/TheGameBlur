using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerMovement : MonoBehaviour
{
    private const float WalkSfxInterval = 1.2f;
    private const float RunSfxInterval = 0.6f;

    [SerializeField] private Transform visualRoot;
    [SerializeField] private PlayerGroundSensor groundSensor;

    public bool IsGrounded { get; private set; }
    public bool IsDashing => dashRemaining > 0f;

    public int FacingSign { get; private set; } = 1;
    public int LastMoveSign { get; private set; } = 1;

    public int MoveSign => moveSign;
    public bool RunHeld => runActive;

    private Player player;
    private Rigidbody body;
    private Collider bodyCollider;
    private PlayerSettings settings;
    private PlayerStats stats;
    private PlayerCombat combat;
    private InputManager input;

    private int moveSign;
    private bool runHeldRaw;
    private bool runActive;
    private bool jumpDown;
    private bool jumpUp;
    private bool jumpHeld;
    private bool dashDown;

    private float runLockRemaining;

    private float groundAccelElapsed;
    private int lastGroundMoveSign;

    private float coyoteRemaining;

    private bool jumpHoldActive;
    private float jumpHoldElapsed;

    private float dashCooldownRemaining;
    private float dashRemaining;
    private bool isAirDash;
    private bool hasUsedAirDash;
    private int dashSign;
    private RigidbodyConstraints dashPrevConstraints;

    private Vector3 pendingImpulse;

    private bool isStandingUpFromChair;

    private float rightFacingY;
    private Tween facingTween;

    private bool lastGroundedForLand;
    private bool lastFootstepActive;
    private float footstepRemaining;

    private void Start()
    {
        player = Player.Instance;

        body = player.Body;
        bodyCollider = player.BodyCollider;
        settings = player.Settings;
        stats = player.Stats;
        combat = player.Combat;
        input = player.Input;

        groundSensor.Initialize(settings.groundMask, player.transform);

        body.useGravity = false;

        body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        body.constraints |= RigidbodyConstraints.FreezePositionZ;

        InitializeVisualFacing();

        UpdateGrounded();
        lastGroundedForLand = IsGrounded;
        ResetFootstepState();
    }

    private void InitializeVisualFacing()
    {
        if (visualRoot == null) return;

        rightFacingY = visualRoot.localEulerAngles.y;

        Vector3 s = visualRoot.localScale;
        s.x = Mathf.Abs(s.x);
        visualRoot.localScale = s;

        ApplyVisualFacing(true);
    }

    private void Update()
    {
        if (!player.Stats.IsActive) return;

        float dt = Time.deltaTime;
        if (runLockRemaining > 0f)
        {
            runLockRemaining -= dt;
            if (runLockRemaining < 0f) runLockRemaining = 0f;
        }

        float axis = input.MoveAxis;

        moveSign = 0;
        if (axis > settings.moveDeadZone) moveSign = 1;
        else if (axis < -settings.moveDeadZone) moveSign = -1;

        if (player.IsSitting)
        {
            if (!isStandingUpFromChair && moveSign != 0) player.RequestStandUpFromChair();

            moveSign = 0;

            runHeldRaw = false;
            runActive = false;
            runLockRemaining = 0f;
            jumpDown = false;
            jumpUp = false;
            jumpHeld = false;
            dashDown = false;

            return;
        }

        if (moveSign != 0) LastMoveSign = moveSign;

        runHeldRaw = input.RunHeld;
        runActive = runHeldRaw && moveSign != 0 && runLockRemaining <= 0f;

        jumpDown = input.JumpDown;
        jumpUp = input.JumpUp;
        jumpHeld = input.JumpHeld;

        dashDown = input.DashDown;

        UpdateFacing();
    }

    private void FixedUpdate()
    {
        if (!player.Stats.IsActive) return;

        float dt = Time.fixedDeltaTime;

        UpdateGrounded();
        TryPlayLandSfx();

        if (dashCooldownRemaining > 0f) dashCooldownRemaining -= dt;

        if (player.IsSitting)
        {
            body.linearVelocity = Vector3.zero;
            pendingImpulse = Vector3.zero;
            LockPlaneZ();
            ResetFootstepState();
            return;
        }

        if (combat.IsUltimateActive)
        {
            body.linearVelocity = Vector3.zero;
            pendingImpulse = Vector3.zero;
            LockPlaneZ();

            ResetFootstepState();

            dashDown = false;
            jumpDown = false;
            jumpUp = false;
            return;
        }

        if (IsDashing)
        {
            TickDash(dt);
            ApplyPendingImpulse();
            LockPlaneZ();
            ResetFootstepState();
            return;
        }

        if (dashDown) TryStartDash();

        TickJump(dt);

        float vx = ComputeHorizontalVelocity(dt);
        float vy = ComputeVerticalVelocity(dt);

        Vector3 v = body.linearVelocity;
        v.x = vx;
        v.y = vy;
        v.z = 0f;

        body.linearVelocity = v;

        ApplyPendingImpulse();
        LockPlaneZ();

        TickFootsteps(dt);

        dashDown = false;
        jumpDown = false;
        jumpUp = false;
    }

    public void EnterSitting()
    {
        if (IsDashing)
        {
            dashRemaining = 0f;

            if (isAirDash)
            {
                body.constraints = dashPrevConstraints;
                isAirDash = false;
            }
        }

        dashCooldownRemaining = 0f;
        jumpHoldActive = false;
        jumpHoldElapsed = 0f;
        coyoteRemaining = 0f;

        pendingImpulse = Vector3.zero;
        body.linearVelocity = Vector3.zero;

        FacingSign = 1;
        isStandingUpFromChair = false;

        runHeldRaw = false;
        runActive = false;
        runLockRemaining = 0f;

        ResetFootstepState();

        ApplyVisualFacing();
    }

    public void NotifyBasicAttackStarted(float duration)
        => NotifyBasicAttackStarted(duration, true);

    public void NotifyBasicAttackStarted(float duration, bool lockRun)
    {
        if (lockRun)
        {
            if (duration > runLockRemaining) runLockRemaining = duration;
            runActive = false;

            if (stats.IsBattle) ForceFaceMouseNow();
        }
    }

    public void CancelBasicAttackRunLock()
    {
        runLockRemaining = 0f;
    }

    public void BeginStandingUpFromChair()
    {
        isStandingUpFromChair = true;
    }

    public void ExitSitting()
    {
        isStandingUpFromChair = false;
    }

    public void AddImpulse(Vector3 impulse)
    {
        pendingImpulse += impulse;
    }

    public void SetVelocity(Vector3 velocity)
    {
        body.linearVelocity = new Vector3(velocity.x, velocity.y, 0f);
        LockPlaneZ();
    }

    public void RefreshAirDashUsage()
    {
        hasUsedAirDash = false;
    }

    public void ApplyAirPogoBounce()
    {
        Vector3 v = body.linearVelocity;

        if (v.y < 0f) v.y = 0f;
        if (v.y < settings.airPogoBounceVelocity) v.y = settings.airPogoBounceVelocity;

        v.z = 0f;
        body.linearVelocity = v;

        LockPlaneZ();

        if (jumpHoldActive) CancelJumpHoldWithoutCut();
    }

    private void CancelJumpHoldWithoutCut()
    {
        jumpHoldActive = false;
        jumpHoldElapsed = settings.maxJumpHoldTime;
    }

    private void TryStartDash()
    {
        if (combat.IsSkillOrUltimateActive) return;
        if (dashCooldownRemaining > 0f) return;

        bool grounded = IsGrounded;

        if (!grounded && hasUsedAirDash) return;

        int sign = moveSign != 0 ? moveSign : LastMoveSign;
        if (sign == 0) sign = FacingSign;

        dashSign = sign;

        dashRemaining = settings.dashDuration;
        dashCooldownRemaining = settings.dashCooldown;

        isAirDash = !grounded;

        if (isAirDash)
        {
            hasUsedAirDash = true;
            dashPrevConstraints = body.constraints;
            body.constraints |= RigidbodyConstraints.FreezePositionY;

            Vector3 v0 = body.linearVelocity;
            v0.y = 0f;
            body.linearVelocity = v0;
        }

        jumpHoldActive = false;
        jumpHoldElapsed = 0f;
        coyoteRemaining = 0f;

        combat.CancelForDash();
        stats.SetInvincible(true);

        float dashSpeed = settings.dashDuration > 0f ? settings.dashDistance / settings.dashDuration : 0f;

        Vector3 v = body.linearVelocity;
        v.x = dashSpeed * dashSign;
        if (isAirDash) v.y = 0f;
        v.z = 0f;

        body.linearVelocity = v;

        AudioManager.Instance.PlaySFX("Dash");

        dashDown = false;
        jumpDown = false;
        jumpUp = false;
    }

    private void TickDash(float dt)
    {
        dashRemaining -= dt;

        float dashSpeed = settings.dashDuration > 0f ? settings.dashDistance / settings.dashDuration : 0f;

        Vector3 v = body.linearVelocity;
        v.x = dashSpeed * dashSign;
        if (isAirDash) v.y = 0f;
        v.z = 0f;

        body.linearVelocity = v;

        if (dashRemaining > 0f) return;

        dashRemaining = 0f;

        if (isAirDash)
        {
            body.constraints = dashPrevConstraints;

            Vector3 endV = body.linearVelocity;
            endV.y = 0f;
            endV.z = 0f;
            body.linearVelocity = endV;

            isAirDash = false;
        }

        body.linearVelocity = Vector3.zero;

        stats.SetInvincible(false);
    }

    private void TickJump(float dt)
    {
        if (IsGrounded)
        {
            coyoteRemaining = settings.coyoteTime;

            if (hasUsedAirDash)
                hasUsedAirDash = false;
        }
        else
        {
            if (coyoteRemaining > 0f) coyoteRemaining -= dt;
        }

        if (jumpDown)
        {
            bool canJump = IsGrounded || coyoteRemaining > 0f;

            if (canJump)
            {
                Vector3 v = body.linearVelocity;
                if (v.y < 0f) v.y = 0f;
                body.linearVelocity = v;

                jumpHoldActive = true;
                jumpHoldElapsed = 0f;
                coyoteRemaining = 0f;
            }
        }

        if (jumpUp)
        {
            jumpHoldActive = false;
            CutJumpRise();
        }

        if (!jumpHoldActive) return;

        if (!jumpHeld)
        {
            jumpHoldActive = false;
            CutJumpRise();
            return;
        }

        if (jumpHoldElapsed >= settings.maxJumpHoldTime)
        {
            jumpHoldActive = false;
            return;
        }

        float t = settings.maxJumpHoldTime > 0f ? jumpHoldElapsed / settings.maxJumpHoldTime : 1f;
        float s = settings.jumpForceCurve != null ? settings.jumpForceCurve.Evaluate(t) : 1f;

        Vector3 v2 = body.linearVelocity;
        v2.y += s * settings.maxJumpForce * dt;
        body.linearVelocity = v2;

        jumpHoldElapsed += dt;
    }

    private void CutJumpRise()
    {
        Vector3 v = body.linearVelocity;
        if (v.y > 0f) v.y = 0f;
        body.linearVelocity = v;
    }

    private float ComputeHorizontalVelocity(float dt)
    {
        float targetSpeed = settings.baseMoveSpeed;
        if (runActive) targetSpeed *= settings.runSpeedMultiplier;

        if (IsGrounded && !runActive && moveSign != 0 && moveSign != FacingSign)
            targetSpeed *= settings.backwardMoveSpeedMultiplier;

        float current = body.linearVelocity.x;

        if (IsGrounded) return ComputeGroundVx(dt, targetSpeed);

        return ComputeAirVx(dt, targetSpeed, current);
    }

    private float ComputeGroundVx(float dt, float targetSpeed)
    {
        if (moveSign == 0)
        {
            groundAccelElapsed = 0f;
            lastGroundMoveSign = 0;
            return 0f;
        }

        if (lastGroundMoveSign != moveSign)
        {
            lastGroundMoveSign = moveSign;
            groundAccelElapsed = 0f;
        }

        groundAccelElapsed += dt;

        float t = settings.groundAccelTimeToMax > 0f ? Mathf.Clamp01(groundAccelElapsed / settings.groundAccelTimeToMax) : 1f;
        float mul = Mathf.Lerp(settings.groundStartSpeedMultiplier, 1f, t);

        return moveSign * targetSpeed * mul;
    }

    private float ComputeAirVx(float dt, float targetSpeed, float currentVx)
    {
        groundAccelElapsed = 0f;
        lastGroundMoveSign = 0;

        if (moveSign != 0)
        {
            float accelTime = settings.airAccelTimeToMax > 0f ? settings.airAccelTimeToMax : dt;
            float accel = targetSpeed / accelTime;
            return Mathf.MoveTowards(currentVx, moveSign * targetSpeed, accel * dt);
        }

        return Mathf.MoveTowards(currentVx, 0f, settings.airDecel * dt);
    }

    private float ComputeVerticalVelocity(float dt)
    {
        if (IsDashing && isAirDash) return 0f;

        float vy = body.linearVelocity.y;

        vy += settings.gravity * dt;

        float maxFall = -Mathf.Abs(settings.maxFallSpeed);
        if (vy < maxFall) vy = maxFall;

        return vy;
    }

    private void UpdateGrounded()
    {
        bool touching = groundSensor.IsTouchingGround;
        bool grounded = touching && body.linearVelocity.y <= settings.groundedMinUpVelocity;
        IsGrounded = grounded;
    }

    private void TryPlayLandSfx()
    {
        if (!lastGroundedForLand && IsGrounded && !player.IsSitting)
            AudioManager.Instance.PlaySFX("Land");

        lastGroundedForLand = IsGrounded;
    }

    private void TickFootsteps(float dt)
    {
        bool active = IsGrounded && moveSign != 0;

        if (!active)
        {
            ResetFootstepState();
            return;
        }

        if (!lastFootstepActive) footstepRemaining = 0f;

        float interval = runActive ? RunSfxInterval : WalkSfxInterval;

        footstepRemaining -= dt;

        if (footstepRemaining <= 0f)
        {
            AudioManager.Instance.PlaySFX("Walk");
            footstepRemaining += interval;
        }

        lastFootstepActive = true;
    }

    private void ResetFootstepState()
    {
        lastFootstepActive = false;
        footstepRemaining = 0f;
    }

    private void UpdateFacing()
    {
        int desired = FacingSign;

        if (stats.IsBattle && !runActive)
        {
            if (TryGetMouseWorldOnPlane(out Vector3 p))
            {
                float dx = p.x - body.position.x;

                if (dx > settings.mouseFacingDeadZone) desired = 1;
                else if (dx < -settings.mouseFacingDeadZone) desired = -1;
            }
        }
        else
        {
            if (moveSign != 0) desired = moveSign;
        }

        if (desired == FacingSign) return;

        FacingSign = desired;
        ApplyVisualFacing();
    }

    private void ForceFaceMouseNow()
    {
        if (!TryGetMouseWorldOnPlane(out Vector3 p)) return;

        float dx = p.x - body.position.x;

        int desired = FacingSign;
        if (dx > settings.mouseFacingDeadZone) desired = 1;
        else if (dx < -settings.mouseFacingDeadZone) desired = -1;

        if (desired == FacingSign) return;

        FacingSign = desired;
        ApplyVisualFacing();
    }

    private bool TryGetMouseWorldOnPlane(out Vector3 world)
    {
        Camera cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, settings.planeZ));

        float enter;
        if (!plane.Raycast(ray, out enter))
        {
            world = default;
            return false;
        }

        world = ray.GetPoint(enter);
        return true;
    }

    private void ApplyVisualFacing()
    {
        ApplyVisualFacing(false);
    }

    private void ApplyVisualFacing(bool instant)
    {
        if (visualRoot == null) return;

        Vector3 s = visualRoot.localScale;
        s.x = Mathf.Abs(s.x);
        visualRoot.localScale = s;

        float y = FacingSign < 0 ? rightFacingY + 180f : rightFacingY;
        y = Mathf.Repeat(y, 360f);

        Vector3 target = visualRoot.localEulerAngles;
        target.y = y;

        if (facingTween != null) facingTween.Kill();

        float duration = instant ? 0f : settings.facingTurnDuration;
        if (duration <= 0f)
            visualRoot.localEulerAngles = target;
        else
            facingTween = visualRoot.DOLocalRotate(target, duration, RotateMode.Fast).SetEase(Ease.OutSine);
    }

    private void ApplyPendingImpulse()
    {
        if (pendingImpulse == Vector3.zero) return;

        Vector3 v = body.linearVelocity;
        v += pendingImpulse;
        v.z = 0f;

        body.linearVelocity = v;
        pendingImpulse = Vector3.zero;
    }

    private void LockPlaneZ()
    {
        Vector3 p = body.position;
        if (Mathf.Approximately(p.z, settings.planeZ)) return;

        p.z = settings.planeZ;
        body.position = p;
    }
}