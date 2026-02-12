using UnityEngine;

public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;

    public bool IsGrounded { get; private set; }
    public bool IsDashing => dashRemaining > 0f;
    public int FacingSign { get; private set; } = 1;
    public int LastMoveSign { get; private set; } = 1;

    private Rigidbody body;
    private Collider bodyCollider;
    private PlayerSettings settings;
    private PlayerStats stats;
    private PlayerCombat combat;
    private InputManager input;

    private int moveSign;
    private bool runHeld;
    private bool jumpDown;
    private bool jumpUp;
    private bool jumpHeld;
    private bool dashDown;

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

    private void Start()
    {
        Player player = Player.Instance;

        body = player.Body;
        bodyCollider = player.BodyCollider;
        settings = player.Settings;
        stats = player.Stats;
        combat = player.Combat;
        input = player.Input;

        body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        body.constraints |= RigidbodyConstraints.FreezePositionZ;
    }

    private void Update()
    {
        float axis = input.MoveAxis;

        moveSign = 0;
        if (axis > settings.MoveDeadZone) moveSign = 1;
        else if (axis < -settings.MoveDeadZone) moveSign = -1;

        if (moveSign != 0) LastMoveSign = moveSign;

        runHeld = input.RunHeld;

        jumpDown = input.JumpDown;
        jumpUp = input.JumpUp;
        jumpHeld = input.JumpHeld;

        dashDown = input.DashDown;

        UpdateFacing();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        UpdateGrounded(dt);

        if (dashCooldownRemaining > 0f) dashCooldownRemaining -= dt;

        if (IsDashing)
        {
            TickDash(dt);
            ApplyPendingImpulse();
            LockPlaneZ();
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

        dashDown = false;
        jumpDown = false;
        jumpUp = false;
    }

    public void AddImpulse(Vector3 impulse) => pendingImpulse += impulse;

    public void SetVelocity(Vector3 velocity)
    {
        body.linearVelocity = new Vector3(velocity.x, velocity.y, 0f);
        LockPlaneZ();
    }

    private void TryStartDash()
    {
        if (dashCooldownRemaining > 0f) return;
        if (combat.IsSkillOrUltimateActive) return;

        bool grounded = IsGrounded;

        if (!grounded && hasUsedAirDash) return;

        int sign = moveSign != 0 ? moveSign : LastMoveSign;
        if (sign == 0) sign = FacingSign;

        dashSign = sign;

        dashRemaining = settings.DashDuration;
        dashCooldownRemaining = settings.DashCooldown;

        isAirDash = !grounded;

        if (isAirDash)
        {
            hasUsedAirDash = true;
            dashPrevConstraints = body.constraints;
            body.constraints |= RigidbodyConstraints.FreezePositionY;
            body.useGravity = false;
        }

        jumpHoldActive = false;
        jumpHoldElapsed = 0f;
        coyoteRemaining = 0f;

        combat.CancelForDash();
        stats.SetInvincible(true);

        float dashSpeed = settings.DashDuration > 0f ? settings.DashDistance / settings.DashDuration : 0f;

        Vector3 v = body.linearVelocity;
        v.x = dashSpeed * dashSign;
        if (isAirDash) v.y = 0f;
        v.z = 0f;

        body.linearVelocity = v;

        dashDown = false;
        jumpDown = false;
        jumpUp = false;
    }

    private void TickDash(float dt)
    {
        dashRemaining -= dt;

        float dashSpeed = settings.DashDuration > 0f ? settings.DashDistance / settings.DashDuration : 0f;

        Vector3 v = body.linearVelocity;
        v.x = dashSpeed * dashSign;
        if (isAirDash) v.y = 0f;
        v.z = 0f;

        body.linearVelocity = v;

        if (dashRemaining > 0f) return;

        dashRemaining = 0f;

        if (isAirDash)
        {
            body.useGravity = true;
            body.constraints = dashPrevConstraints;

            Vector3 endV = body.linearVelocity;
            endV.y = 0f;
            body.linearVelocity = endV;

            isAirDash = false;
        }

        stats.SetInvincible(false);
    }

    private void TickJump(float dt)
    {
        if (IsGrounded)
        {
            coyoteRemaining = settings.CoyoteTime;
            if (hasUsedAirDash) hasUsedAirDash = false;
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

        if (jumpUp) jumpHoldActive = false;

        if (!jumpHoldActive) return;

        if (!jumpHeld)
        {
            jumpHoldActive = false;
            return;
        }

        if (jumpHoldElapsed >= settings.MaxJumpHoldTime)
        {
            jumpHoldActive = false;
            return;
        }

        float t = settings.MaxJumpHoldTime > 0f ? jumpHoldElapsed / settings.MaxJumpHoldTime : 1f;
        float s = settings.JumpForceCurve != null ? settings.JumpForceCurve.Evaluate(t) : 1f;

        Vector3 v2 = body.linearVelocity;
        v2.y += s * settings.MaxJumpForce * dt;
        body.linearVelocity = v2;

        jumpHoldElapsed += dt;
    }

    private float ComputeHorizontalVelocity(float dt)
    {
        float targetSpeed = settings.BaseMoveSpeed;
        if (runHeld) targetSpeed *= settings.RunSpeedMultiplier;

        if (!runHeld && moveSign != 0 && moveSign != FacingSign)
            targetSpeed *= settings.BackwardMoveSpeedMultiplier;

        float current = body.linearVelocity.x;

        if (IsGrounded)
            return ComputeGroundVx(dt, targetSpeed);

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

        float t = settings.GroundAccelTimeToMax > 0f ? Mathf.Clamp01(groundAccelElapsed / settings.GroundAccelTimeToMax) : 1f;
        float mul = Mathf.Lerp(settings.GroundStartSpeedMultiplier, 1f, t);

        return moveSign * targetSpeed * mul;
    }

    private float ComputeAirVx(float dt, float targetSpeed, float currentVx)
    {
        groundAccelElapsed = 0f;
        lastGroundMoveSign = 0;

        if (moveSign != 0)
        {
            float accelTime = settings.AirAccelTimeToMax > 0f ? settings.AirAccelTimeToMax : dt;
            float accel = targetSpeed / accelTime;
            return Mathf.MoveTowards(currentVx, moveSign * targetSpeed, accel * dt);
        }

        return Mathf.MoveTowards(currentVx, 0f, settings.AirDecel * dt);
    }

    private float ComputeVerticalVelocity(float dt)
    {
        if (IsDashing && isAirDash) return 0f;

        float vy = body.linearVelocity.y;

        vy += settings.Gravity * dt;

        float maxFall = -Mathf.Abs(settings.MaxFallSpeed);
        if (vy < maxFall) vy = maxFall;

        return vy;
    }

    private void UpdateGrounded(float dt)
    {
        bool grounded = false;

        if (body.linearVelocity.y <= settings.GroundedMinUpVelocity)
        {
            RaycastHit hit;
            grounded = CheckGround(out hit);
        }

        IsGrounded = grounded;
    }

    private bool CheckGround(out RaycastHit hit)
    {
        if (bodyCollider is CapsuleCollider capsule)
        {
            Transform t = bodyCollider.transform;

            Vector3 center = t.TransformPoint(capsule.center);

            float scaleX = t.lossyScale.x;
            float scaleY = t.lossyScale.y;
            float scaleZ = t.lossyScale.z;

            float radius = capsule.radius * Mathf.Max(scaleX, scaleZ);
            float height = Mathf.Max(capsule.height * scaleY, radius * 2f);

            Vector3 up = t.up;

            float half = height * 0.5f - radius;
            Vector3 p1 = center + up * half;
            Vector3 p2 = center - up * half;

            return Physics.CapsuleCast(p1, p2, radius, -up, out hit, settings.GroundCheckDistance, settings.GroundMask, QueryTriggerInteraction.Ignore);
        }

        Bounds b = bodyCollider.bounds;
        float r = Mathf.Min(b.extents.x, b.extents.z);

        Vector3 origin = b.center;
        float dist = b.extents.y + settings.GroundCheckDistance;

        return Physics.SphereCast(origin, r, Vector3.down, out hit, dist, settings.GroundMask, QueryTriggerInteraction.Ignore);
    }

    private void UpdateFacing()
    {
        int desired = FacingSign;

        if (stats.IsBattle)
        {
            if (TryGetMouseWorldOnPlane(out Vector3 p))
            {
                float dx = p.x - body.position.x;

                if (dx > settings.MouseFacingDeadZone) desired = 1;
                else if (dx < -settings.MouseFacingDeadZone) desired = -1;
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

    private bool TryGetMouseWorldOnPlane(out Vector3 world)
    {
        Camera cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(UnityEngine.Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, settings.PlaneZ));

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
        if (visualRoot == null) return;

        Vector3 s = visualRoot.localScale;
        s.x = Mathf.Abs(s.x) * FacingSign;
        visualRoot.localScale = s;
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
        if (Mathf.Approximately(p.z, settings.PlaneZ)) return;

        p.z = settings.PlaneZ;
        body.position = p;
    }
}