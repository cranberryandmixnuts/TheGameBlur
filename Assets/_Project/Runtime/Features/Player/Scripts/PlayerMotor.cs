// PlayerMotor.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public sealed class PlayerMotor : MonoBehaviour
{
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.22f;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private Transform headSensor;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private PlayerSettings settings;

    private float desiredAxis;
    private float desiredSpeed;
    private float accelTimeToMax;

    private bool isGrounded;

    private float dashCooldownRemaining;
    private float dashRemaining;
    private float dashSpeed;
    private int dashDir;
    private float dashEndMoveSpeed;
    private bool isDashing;
    private bool isInvincible;
    private bool isAirDashConsumed;

    private bool isLedgeAssisting;
    private float ledgeAssistRemaining;
    private float ledgeAssistDuration;
    private Vector3 ledgeAssistFrom;
    private Vector3 ledgeAssistTo;

    private bool prevUseGravity;

    private bool jumpHeld;
    private float jumpHoldRemaining;
    private float jumpHoldElapsed;

    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;
    public bool IsInvincible => isInvincible;
    public bool IsLedgeAssisting => isLedgeAssisting;
    public int DashDir => dashDir;
    public Vector3 Velocity => rb.linearVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.useGravity = true;
        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;
    }

    public void SetSettings(PlayerSettings settings)
    {
        this.settings = settings;
    }

    public void SetFacingDir(int dir)
    {
        if (dir < 0)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        else
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void SetMove(float axis, float speed, float accelTime)
    {
        desiredAxis = Mathf.Clamp(axis, -1f, 1f);
        desiredSpeed = speed;
        accelTimeToMax = accelTime;
    }

    public void SetJumpHeld(bool held)
    {
        jumpHeld = held;
    }

    public void Jump()
    {
        Vector3 v = rb.linearVelocity;
        if (v.y < 0f)
            v.y = 0f;

        float initialUpVelocity = settings.jumpHoldForceCurve.Evaluate(0f) * settings.jumpHeightMultiplier;
        v.y = initialUpVelocity;
        rb.linearVelocity = v;

        jumpHoldRemaining = settings.jumpHoldTime;
        jumpHoldElapsed = 0f;
    }

    public void CutJump(float cutMultiplier)
    {
        jumpHoldRemaining = 0f;

        Vector3 v = rb.linearVelocity;
        if (v.y > 0f)
            v.y *= cutMultiplier;

        rb.linearVelocity = v;
    }

    public bool TryDash(PlayerSettings settings, float dirX)
    {
        if (isLedgeAssisting)
            return false;

        if (isDashing)
            return false;

        if (dashCooldownRemaining > 0f)
            return false;

        if (!isGrounded && isAirDashConsumed)
            return false;

        dashCooldownRemaining = settings.dashCooldown;
        dashRemaining = settings.dashDuration;

        float speed = settings.dashDistance / settings.dashDuration;
        dashSpeed = dirX >= 0f ? speed : -speed;
        dashDir = dashSpeed >= 0f ? 1 : -1;
        dashEndMoveSpeed = settings.moveSpeed;

        isDashing = true;
        isInvincible = true;

        prevUseGravity = rb.useGravity;

        if (!isGrounded)
        {
            isAirDashConsumed = true;
            rb.useGravity = false;

            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
        }

        return true;
    }

    public void ClampFallSpeed(float maxFallSpeed)
    {
        Vector3 v = rb.linearVelocity;
        if (v.y < -maxFallSpeed)
            v.y = -maxFallSpeed;

        rb.linearVelocity = v;
    }

    public void ResetAirDash()
    {
        isAirDashConsumed = false;
    }

    public void ApplyWallRecoil(PlayerSettings settings, Vector3 aimDir)
    {
        Vector3 v = rb.linearVelocity;
        v.x = -Mathf.Sign(aimDir.x) * settings.attackWallRecoilSpeed;
        rb.linearVelocity = v;
    }

    public void ApplyDownAttackBounce(PlayerSettings settings)
    {
        Vector3 v = rb.linearVelocity;
        v.y = settings.downAttackBounceVelocity;
        rb.linearVelocity = v;
    }

    public void TryStartLedgeAssist(PlayerSettings settings, int facingDir)
    {
        if (isLedgeAssisting)
            return;

        if (isGrounded)
            return;

        if (rb.linearVelocity.y > 0f)
            return;

        Vector3 origin = headSensor.position;
        Vector3 dir = facingDir < 0 ? Vector3.left : Vector3.right;

        if (!Physics.SphereCast(origin, settings.ledgeSensorRadius, Vector3.up, out RaycastHit upHit, settings.ledgeProbeUp, groundMask, QueryTriggerInteraction.Ignore))
            return;

        Vector3 forwardOrigin = upHit.point + dir * settings.ledgeProbeForward;
        if (!Physics.Raycast(forwardOrigin, Vector3.down, out RaycastHit downHit, settings.ledgeProbeDown, groundMask, QueryTriggerInteraction.Ignore))
            return;

        float extY = capsule.height * 0.5f;
        Vector3 target =
            new Vector3(
                downHit.point.x - facingDir * settings.ledgeSnapInset,
                downHit.point.y + extY + settings.ledgeSnapExtraY,
                rb.position.z
            );

        isLedgeAssisting = true;
        ledgeAssistDuration = settings.ledgeAssistDuration;
        ledgeAssistRemaining = settings.ledgeAssistDuration;

        ledgeAssistFrom = rb.position;
        ledgeAssistTo = target;

        prevUseGravity = rb.useGravity;
        rb.useGravity = false;

        Vector3 v = rb.linearVelocity;
        v.x = 0f;
        v.y = 0f;
        rb.linearVelocity = v;
    }

    private void FixedUpdate()
    {
        UpdateGround();

        float dt = Time.fixedDeltaTime;

        if (rb.useGravity && settings != null)
            rb.AddForce(Physics.gravity * (settings.gravityMultiplier - 1f), ForceMode.Acceleration);

        if (jumpHeld && jumpHoldRemaining > 0f && rb.linearVelocity.y > 0f)
        {
            float t = settings.jumpHoldTime > 0f ? jumpHoldElapsed / settings.jumpHoldTime : 1f;
            if (t > 1f)
                t = 1f;

            float accel = settings.jumpHoldForceCurve.Evaluate(t) * settings.jumpHeightMultiplier;
            rb.AddForce(Vector3.up * accel, ForceMode.Acceleration);

            jumpHoldElapsed += dt;
            jumpHoldRemaining -= dt;
        }

        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining -= dt;
            if (dashCooldownRemaining < 0f)
                dashCooldownRemaining = 0f;
        }

        if (isLedgeAssisting)
        {
            ledgeAssistRemaining -= dt;

            float t = 1f - Mathf.Clamp01(ledgeAssistRemaining / ledgeAssistDuration);
            rb.MovePosition(Vector3.Lerp(ledgeAssistFrom, ledgeAssistTo, t));

            if (ledgeAssistRemaining <= 0f)
            {
                isLedgeAssisting = false;
                rb.useGravity = prevUseGravity;
            }

            return;
        }

        if (isDashing)
        {
            dashRemaining -= dt;

            Vector3 v = rb.linearVelocity;
            v.x = dashSpeed;
            v.y = 0f;
            v.z = 0f;
            rb.linearVelocity = v;

            if (dashRemaining <= 0f)
            {
                isDashing = false;
                isInvincible = false;

                rb.useGravity = prevUseGravity;

                Vector3 after = rb.linearVelocity;
                after.z = 0f;

                if (Mathf.Abs(desiredAxis) > 0.001f && Mathf.Sign(desiredAxis) == dashDir)
                    after.x = dashDir * dashEndMoveSpeed;
                else
                    after.x = 0f;

                rb.linearVelocity = after;
            }

            return;
        }

        float currentX = rb.linearVelocity.x;

        if (Mathf.Abs(desiredAxis) < 0.001f)
            currentX = 0f;
        else
        {
            float targetX = desiredAxis * desiredSpeed;
            float maxDelta = accelTimeToMax > 0f ? (desiredSpeed / accelTimeToMax) * dt : float.MaxValue;
            currentX = Mathf.MoveTowards(currentX, targetX, maxDelta);
        }

        Vector3 vel = rb.linearVelocity;
        vel.x = currentX;
        vel.z = 0f;
        rb.linearVelocity = vel;
    }

    private void UpdateGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        if (isGrounded)
            ResetAirDash();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.isTrigger)
            return;

        int count = collision.contactCount;

        for (int i = 0; i < count; i++)
        {
            Vector3 n = collision.GetContact(i).normal;
            n.z = 0f;

            if (Mathf.Abs(n.x) >= 0.7f && n.y <= 0.2f)
            {
                ResetAirDash();
                return;
            }
        }
    }
}