using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class SideScrollerCameraController : Singleton<SideScrollerCameraController, SceneScope>
{
    [SerializeField] private PlayerSettings settings;
    [SerializeField] private Player player;
    [SerializeField] private Transform target;
    [SerializeField] private CameraBounds bounds;

    [Header("Move Detection")]
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Snap Control")]
    [SerializeField] private float teleportSnapDistance = 5f;

    private Camera cam;
    private PlayerMovement movement;
    private PlayerCombat combat;
    private InputManager input;

    private float lookAheadCurrent;
    private float yOffsetCurrent;
    private float lookDownHold;
    private float cliffLookDownRemaining;

    private Tween lookAheadTween;
    private Tween yOffsetTween;

    private bool locked;
    private Vector3 lockPos;

    private float shakeRemaining;
    private float shakeAmplitude;
    private float shakeFrequency;
    private float shakeTime;

    private bool isMovingThisFrame;

    private void Start()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (player == null)
            player = Player.Instance;

        movement = player.Movement;
        combat = player.Combat;
        input = player.Input;

        if (target == null)
            target = player.transform;
    }

    private void OnDisable()
    {
        lookAheadTween?.Kill();
        yOffsetTween?.Kill();
    }

    private void LateUpdate()
    {
        Vector3 desired = locked ? lockPos : ComputeFollowPosition();
        if (bounds != null)
            desired = bounds.Clamp(desired, cam);

        float dx = desired.x - transform.position.x;
        float dy = desired.y - transform.position.y;
        float distSq = dx * dx + dy * dy;
        float teleportSq = teleportSnapDistance * teleportSnapDistance;

        bool hasOffsetTween =
            (lookAheadTween != null && lookAheadTween.IsActive() && lookAheadTween.IsPlaying()) ||
            (yOffsetTween != null && yOffsetTween.IsActive() && yOffsetTween.IsPlaying());

        bool followSmooth = isMovingThisFrame || hasOffsetTween;

        Vector3 p;
        if (!followSmooth || distSq >= teleportSq)
            p = desired;
        else
            p = Vector3.MoveTowards(transform.position, desired, settings.cameraFollowSpeed * Time.deltaTime);

        p.z = transform.position.z;
        transform.position = p + ComputeShake();
    }

    private Vector3 ComputeFollowPosition()
    {
        float screenW = cam.orthographicSize * 2f * cam.aspect;
        float screenH = cam.orthographicSize * 2f;

        isMovingThisFrame = Mathf.Abs(input.MoveAxis) > moveThreshold;

        int facingSign = movement.FacingSign;
        if (facingSign == 0)
            facingSign = 1;

        float lookAheadTarget = facingSign * (screenW * settings.lookAheadPercent);
        TweenToLookAhead(lookAheadTarget);

        float cliffTarget = GetCliffLookDownOffset(screenH);
        float manualTarget = GetManualLookDownOffset(screenH);
        float yTarget = cliffTarget + manualTarget;

        TweenToYOffset(yTarget);

        Vector3 basePos = GetTargetPosition();
        return new Vector3(basePos.x + lookAheadCurrent, basePos.y + yOffsetCurrent, transform.position.z);
    }

    private float GetManualLookDownOffset(float screenH)
    {
        bool holdingDown = input.MoveVector.y < -0.8f;

        bool pureIdle =
            movement.IsGrounded &&
            !movement.IsDashing &&
            !combat.IsSkillOrUltimateActive &&
            !player.IsSitting &&
            Mathf.Abs(input.MoveAxis) < 0.05f &&
            !input.RunHeld &&
            !input.DashDown &&
            !input.AttackDown &&
            !input.SkillDown &&
            !input.DiceSkillDown &&
            !input.JumpDown &&
            !input.JumpUp &&
            holdingDown;

        if (pureIdle)
            lookDownHold += Time.deltaTime;
        else
            lookDownHold = 0f;

        if (lookDownHold < settings.lookDownHoldTime)
            return 0f;

        Vector3 basePos = GetTargetPosition();
        float desiredOffset = -(screenH * settings.lookDownPercent);

        return GetClampedYOffset(basePos, desiredOffset);
    }

    private float GetCliffLookDownOffset(float screenH)
    {
        if (ShouldStartCliffLookDown())
            cliffLookDownRemaining = settings.cliffLookDownGraceTime;
        else if (cliffLookDownRemaining > 0f)
            cliffLookDownRemaining -= Time.deltaTime;

        if (cliffLookDownRemaining <= 0f)
            return 0f;

        Vector3 basePos = GetTargetPosition();
        float desiredOffset = -(screenH * settings.cliffDownPercent);

        return GetClampedYOffset(basePos, desiredOffset);
    }

    private bool ShouldStartCliffLookDown()
    {
        if (!movement.IsGrounded)
            return false;

        if (movement.IsDashing)
            return false;

        Bounds actorBounds = player.BodyCollider.bounds;

        float footY = actorBounds.min.y + 0.05f;
        float leftX = actorBounds.center.x - settings.cliffProbeForward;
        float rightX = actorBounds.center.x + settings.cliffProbeForward;

        Vector3 leftProbeOrigin = new(leftX, footY, settings.planeZ);
        Vector3 rightProbeOrigin = new(rightX, footY, settings.planeZ);

        bool leftHasGround = Physics.Raycast(
            leftProbeOrigin,
            Vector3.down,
            settings.cliffProbeDown,
            settings.groundMask,
            QueryTriggerInteraction.Ignore
        );

        bool rightHasGround = Physics.Raycast(
            rightProbeOrigin,
            Vector3.down,
            settings.cliffProbeDown,
            settings.groundMask,
            QueryTriggerInteraction.Ignore
        );

        return !leftHasGround || !rightHasGround;
    }

    private float GetClampedYOffset(Vector3 basePos, float desiredOffset)
    {
        if (bounds == null)
            return desiredOffset;

        Vector3 probe = new(basePos.x + lookAheadCurrent, basePos.y + desiredOffset, transform.position.z);
        Vector3 clamped = bounds.Clamp(probe, cam);

        return clamped.y - basePos.y;
    }

    private void TweenToLookAhead(float targetValue)
    {
        if (Mathf.Abs(lookAheadCurrent - targetValue) < 0.001f)
            return;

        lookAheadTween?.Kill();
        lookAheadTween =
            DOTween.To(
                    () => lookAheadCurrent,
                    v => lookAheadCurrent = v,
                    targetValue,
                    settings.cameraSmoothTime
                )
                .SetEase(Ease.Linear);
    }

    private void TweenToYOffset(float targetValue)
    {
        if (Mathf.Abs(yOffsetCurrent - targetValue) < 0.001f)
            return;

        yOffsetTween?.Kill();
        yOffsetTween =
            DOTween.To(
                    () => yOffsetCurrent,
                    v => yOffsetCurrent = v,
                    targetValue,
                    settings.cameraSmoothTime
                )
                .SetEase(Ease.Linear);
    }

    public void LockAt(Vector3 worldPos)
    {
        locked = true;
        lockPos = new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }

    public void Unlock()
    {
        locked = false;
    }

    public void AddShake(float duration, float amplitude, float frequency)
    {
        if (duration > shakeRemaining)
            shakeRemaining = duration;

        shakeAmplitude = amplitude;
        shakeFrequency = frequency;
    }

    private Vector3 ComputeShake()
    {
        if (shakeRemaining <= 0f)
            return Vector3.zero;

        shakeRemaining -= Time.deltaTime;
        shakeTime += Time.deltaTime * shakeFrequency;

        float x = (Mathf.PerlinNoise(shakeTime, 0.12f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(0.34f, shakeTime) - 0.5f) * 2f;

        return new Vector3(x, y, 0f) * shakeAmplitude;
    }

    private Vector3 GetTargetPosition()
    {
        if (target != null)
            return target.position;

        return player.Body.position;
    }
}