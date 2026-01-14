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

    private float desiredAxis;
    private float desiredSpeed;
    private float accelTimeToMax;

    private bool isGrounded;

    private float dashCooldownRemaining;
    private float dashRemaining;
    private float dashSpeed;
    private bool isDashing;
    private bool isInvincible;
    private bool isAirDashConsumed;

    private bool isLedgeAssisting;
    private float ledgeAssistRemaining;
    private float ledgeAssistDuration;
    private Vector3 ledgeAssistFrom;
    private Vector3 ledgeAssistTo;

    private bool prevUseGravity;

    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;
    public bool IsInvincible => isInvincible;
    public bool IsLedgeAssisting => isLedgeAssisting;

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

    public void Jump(float jumpVelocity)
    {
        Vector3 v = rb.linearVelocity;
        if (v.y < 0f) v.y = 0f;

        v.y = jumpVelocity;
        rb.linearVelocity = v;
    }

    public void CutJump(float cutMultiplier)
    {
        Vector3 v = rb.linearVelocity;
        if (v.y > 0f) v.y *= cutMultiplier;

        rb.linearVelocity = v;
    }

    public bool TryDash(PlayerSettings settings, float dirX)
    {
        if (isLedgeAssisting) return false;
        if (isDashing) return false;
        if (dashCooldownRemaining > 0f) return false;

        if (!isGrounded && isAirDashConsumed) return false;

        dashCooldownRemaining = settings.dashCooldown;
        dashRemaining = settings.dashDuration;

        float speed = settings.dashDistance / settings.dashDuration;
        dashSpeed = dirX >= 0f ? speed : -speed;

        isDashing = true;
        isInvincible = true;

        if (!isGrounded)
        {
            isAirDashConsumed = true;

            prevUseGravity = rb.useGravity;
            rb.useGravity = false;

            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
        }

        return true;
    }

    public void ResetAirDash()
    {
        isAirDashConsumed = false;
    }

    public void ApplyWallRecoil(PlayerSettings settings, Vector3 aimDir)
    {
        Vector3 v = rb.linearVelocity;
        v.x = -aimDir.x * settings.attackWallRecoilSpeed;
        rb.linearVelocity = v;

        ResetAirDash();
    }

    public void ApplyDownAttackBounce(PlayerSettings settings)
    {
        Vector3 v = rb.linearVelocity;
        v.y = settings.downAttackBounceVelocity;
        rb.linearVelocity = v;

        ResetAirDash();
    }

    public void ClampFallSpeed(float maxFallSpeed)
    {
        Vector3 v = rb.linearVelocity;
        if (v.y < -maxFallSpeed) v.y = -maxFallSpeed;

        rb.linearVelocity = v;
    }

    public bool TryStartLedgeAssist(PlayerSettings settings, int facingDir)
    {
        if (isLedgeAssisting) return false;
        if (isGrounded) return false;
        if (rb.linearVelocity.y >= -0.01f) return false;

        if (!Physics.CheckSphere(headSensor.position, settings.ledgeSensorRadius, groundMask, QueryTriggerInteraction.Ignore))
            return false;

        Vector3 probeStart =
            headSensor.position
            + Vector3.up * settings.ledgeProbeUp
            + Vector3.right * (facingDir * settings.ledgeProbeForward);

        if (!Physics.Raycast(probeStart, Vector3.down, out RaycastHit hit, settings.ledgeProbeDown, groundMask, QueryTriggerInteraction.Ignore))
            return false;

        float extY = capsule.bounds.extents.y;

        Vector3 target =
            new(
                hit.point.x - facingDir * settings.ledgeSnapInset,
                hit.point.y + extY + settings.ledgeSnapExtraY,
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

        return true;
    }

    private void FixedUpdate()
    {
        UpdateGround();

        float dt = Time.fixedDeltaTime;

        if (dashCooldownRemaining > 0f)
            dashCooldownRemaining -= dt;

        if (isLedgeAssisting)
        {
            ledgeAssistRemaining -= dt;

            float t = 1f - Mathf.Clamp01(ledgeAssistRemaining / ledgeAssistDuration);
            Vector3 p = Vector3.Lerp(ledgeAssistFrom, ledgeAssistTo, t);
            rb.MovePosition(p);

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

                rb.useGravity = true;

                Vector3 after = rb.linearVelocity;
                after.z = 0f;
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

    private void OnCollisionStay(Collision collision)
    {
        if (isGrounded) return;
        if (!isAirDashConsumed) return;

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