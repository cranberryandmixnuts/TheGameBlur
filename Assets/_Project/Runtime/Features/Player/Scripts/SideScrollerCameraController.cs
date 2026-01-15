using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class SideScrollerCameraController : Singleton<SideScrollerCameraController, SceneScope>
{
    [SerializeField] private PlayerSettings settings;
    [SerializeField] private Transform target;
    [SerializeField] private PlayerController player;
    [SerializeField] private CameraBounds bounds;
    [SerializeField] private LayerMask groundMask;

    [Header("Move Detection")]
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Snap Control")]
    [SerializeField] private float teleportSnapDistance = 5f;

    private Camera cam;
    private PlayerMotor playerMotor;

    private float lookAheadCurrent;
    private float yOffsetCurrent;

    private float lookDownHold;

    private Tween lookAheadTween;
    private Tween yOffsetTween;

    private bool locked;
    private Vector3 lockPos;

    private float shakeRemaining;
    private float shakeAmplitude;
    private float shakeFrequency;
    private float shakeTime;

    private bool isMovingThisFrame;
    private int moveDir = 1;

    protected override void SingletonAwake()
    {
        cam = GetComponent<Camera>();
        playerMotor = player.GetComponent<PlayerMotor>();
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

        Vector2 move = InputManager.Instance.MoveVector;
        isMovingThisFrame = Mathf.Abs(move.x) > moveThreshold;

        if (isMovingThisFrame)
            moveDir = move.x >= 0f ? 1 : -1;

        float lookAheadTarget = isMovingThisFrame ? moveDir * (screenW * settings.lookAheadPercent) : 0f;
        TweenToLookAhead(lookAheadTarget);

        float cliffTarget = ShouldCliffLookDown() ? -(screenH * settings.cliffDownPercent) : 0f;
        float manualTarget = GetManualLookDownOffset(screenH);
        float yTarget = cliffTarget + manualTarget;

        TweenToYOffset(yTarget);

        float offsetX = lookAheadCurrent;

        if (!isMovingThisFrame)
        {
            float currentOffsetX = transform.position.x - target.position.x;

            if (Mathf.Abs(currentOffsetX) < Mathf.Abs(offsetX) && Mathf.Sign(currentOffsetX) == Mathf.Sign(offsetX))
                offsetX = currentOffsetX;
            else if (Mathf.Sign(currentOffsetX) != Mathf.Sign(offsetX))
                offsetX = 0f;
        }

        Vector3 basePos = target.position;
        return new Vector3(basePos.x + offsetX, basePos.y + yOffsetCurrent, transform.position.z);
    }

    private float GetManualLookDownOffset(float screenH)
    {
        bool holdingDown = InputManager.Instance.MoveVector.y < -0.8f;

        bool pureIdle =
            playerMotor.IsGrounded &&
            !player.IsAttacking &&
            !playerMotor.IsDashing &&
            Mathf.Abs(InputManager.Instance.MoveVector.x) < 0.05f &&
            !InputManager.Instance.RunHeld &&
            !InputManager.Instance.DashDown &&
            !InputManager.Instance.AttackDown &&
            !InputManager.Instance.SkillDown &&
            !InputManager.Instance.DiceSkillDown &&
            !InputManager.Instance.JumpDown &&
            !InputManager.Instance.JumpUp &&
            holdingDown;

        if (pureIdle)
            lookDownHold += Time.deltaTime;
        else
            lookDownHold = 0f;

        if (lookDownHold < settings.lookDownHoldTime)
            return 0f;

        float offset = -(screenH * settings.lookDownPercent);

        if (bounds == null)
            return offset;

        Vector3 probe = new Vector3(target.position.x, target.position.y + offset, transform.position.z);
        Vector3 clamped = bounds.Clamp(probe, cam);

        if (Mathf.Abs(clamped.y - probe.y) > 0.001f)
            return 0f;

        return offset;
    }

    private bool ShouldCliffLookDown()
    {
        if (!playerMotor.IsGrounded)
            return false;

        Vector3 baseOrigin = target.position + Vector3.up * 0.1f;

        Vector3 rightOrigin = baseOrigin + Vector3.right * settings.cliffProbeForward;
        Vector3 leftOrigin = baseOrigin + Vector3.left * settings.cliffProbeForward;

        bool rightHasGround = Physics.Raycast(rightOrigin, Vector3.down, settings.cliffProbeDown, groundMask, QueryTriggerInteraction.Ignore);
        bool leftHasGround = Physics.Raycast(leftOrigin, Vector3.down, settings.cliffProbeDown, groundMask, QueryTriggerInteraction.Ignore);

        return !rightHasGround || !leftHasGround;
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
}