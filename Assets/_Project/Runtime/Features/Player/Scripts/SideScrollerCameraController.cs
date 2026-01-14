using DG.Tweening;
using UnityEngine;

public sealed class SideScrollerCameraController : Singleton<SideScrollerCameraController, SceneScope>
{
    [SerializeField] private PlayerSettings settings;
    [SerializeField] private Transform target;
    [SerializeField] private PlayerController player;
    [SerializeField] private CameraBounds bounds;
    [SerializeField] private LayerMask groundMask;

    [Header("Move Detection")]
    [SerializeField] private float moveThreshold = 0.1f;

    private Camera cam;
    private PlayerMotor playerMotor;

    private Vector3 followVelocity;

    private float lookAheadCurrent;
    private float yOffsetCurrent;

    private float lookDownHold;

    private int moveDir = 1;

    private Tween lookAheadTween;
    private Tween yOffsetTween;

    private bool locked;
    private Vector3 lockPos;

    private float shakeRemaining;
    private float shakeAmplitude;
    private float shakeFrequency;
    private float shakeTime;

    protected override void SingletonAwake()
    {
        cam = Camera.main;
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

        Vector3 p = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, settings.cameraSmoothTime);
        p.z = transform.position.z;

        transform.position = p + ComputeShake();
    }

    private Vector3 ComputeFollowPosition()
    {
        float screenW = cam.orthographicSize * 2f * cam.aspect;
        float screenH = cam.orthographicSize * 2f;

        Vector2 move = InputManager.Instance.MoveVector;

        bool isMoving = Mathf.Abs(move.x) > moveThreshold;
        if (isMoving)
            moveDir = move.x > 0f ? 1 : -1;

        float lookAheadTarget = isMoving ? moveDir * (screenW * settings.lookAheadPercent) : 0f;
        TweenToLookAhead(lookAheadTarget);

        float cliffTarget = ShouldCliffLookDown(isMoving) ? -(screenH * settings.cliffDownPercent) : 0f;
        float manualTarget = GetManualLookDownOffset(screenH, isMoving);
        float yTarget = cliffTarget + manualTarget;

        TweenToYOffset(yTarget);

        Vector3 basePos = target.position;
        return new Vector3(basePos.x + lookAheadCurrent, basePos.y + yOffsetCurrent, transform.position.z);
    }

    private float GetManualLookDownOffset(float screenH, bool isMoving)
    {
        Vector2 move = InputManager.Instance.MoveVector;

        bool holdingDown = move.y < -0.8f;
        bool eligible =
            playerMotor.IsGrounded
            && !isMoving
            && holdingDown
            && IsPureIdleExceptLookDown();

        if (eligible)
            lookDownHold += Time.deltaTime;
        else
            lookDownHold = 0f;

        if (lookDownHold < settings.lookDownHoldTime)
            return 0f;

        float offset = -(screenH * settings.lookDownPercent);

        if (bounds == null)
            return offset;

        Vector3 probe = new(target.position.x, target.position.y + offset, transform.position.z);
        Vector3 clamped = bounds.Clamp(probe, cam);

        if (Mathf.Abs(clamped.y - probe.y) > 0.001f)
            return 0f;

        return offset;
    }

    private bool IsPureIdleExceptLookDown()
    {
        Vector2 move = InputManager.Instance.MoveVector;

        if (Mathf.Abs(move.x) > moveThreshold)
            return false;

        if (InputManager.Instance.RunHeld)
            return false;

        if (InputManager.Instance.JumpHeld)
            return false;

        if (InputManager.Instance.DashDown)
            return false;

        if (InputManager.Instance.AttackDown)
            return false;

        if (InputManager.Instance.SkillDown)
            return false;

        if (InputManager.Instance.DiceSkillDown)
            return false;

        if (InputManager.Instance.HealHeld)
            return false;

        if (InputManager.Instance.InteractionDown)
            return false;

        if (InputManager.Instance.MapDown)
            return false;

        if (InputManager.Instance.EscapeDown)
            return false;

        return true;
    }

    private bool ShouldCliffLookDown(bool isMoving)
    {
        if (!playerMotor.IsGrounded)
            return false;

        if (!isMoving)
            return false;

        Vector3 origin = target.position + moveDir * settings.cliffProbeForward * Vector3.right + Vector3.up * 0.1f;
        return !Physics.Raycast(origin, Vector3.down, settings.cliffProbeDown, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void TweenToLookAhead(float target)
    {
        if (Mathf.Abs(lookAheadCurrent - target) < 0.001f)
            return;

        lookAheadTween?.Kill();
        lookAheadTween = DOTween.To(
                () => lookAheadCurrent,
                v => lookAheadCurrent = v,
                target,
                settings.cameraSmoothTime
            )
            .SetEase(Ease.OutSine);
    }

    private void TweenToYOffset(float target)
    {
        if (Mathf.Abs(yOffsetCurrent - target) < 0.001f)
            return;

        yOffsetTween?.Kill();
        yOffsetTween = DOTween.To(
                () => yOffsetCurrent,
                v => yOffsetCurrent = v,
                target,
                settings.cameraSmoothTime
            )
            .SetEase(Ease.OutSine);
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