using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Game/Player Settings")]
public sealed class PlayerSettings : ScriptableObject
{
    [Header("2.5D")]
    public float fixedZ = 0f;

    [Header("Move")]
    public float moveSpeed = 6f;
    public float runMultiplier = 1.2f;
    public float backwalkMultiplier = 0.8f;
    public float attackMoveMultiplier = 0.5f;
    public float attackBackwalkMultiplier = 0.3f;
    public float accelTimeToMax = 0.05f;

    [Header("Gravity")]
    public float gravityMultiplier = 2f;

    [Header("Jump")]
    public float jumpHoldTime = 0.18f;
    public AnimationCurve jumpHoldForceCurve = AnimationCurve.Linear(0f, 30f, 1f, 0f);
    public float jumpHeightMultiplier = 1f;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;
    public float jumpBufferTime = 0.1f;
    public float coyoteTime = 0.1f;
    public float maxFallSpeed = 25f;

    [Header("Dash")]
    public float dashCooldown = 0.8f;
    public float dashDuration = 0.2f;
    public float dashDistance = 6f;

    [Header("Combat")]
    public float attackAnimTime = 0.2f;
    public float skillAnimTime = 0.2f;
    [Range(0f, 1f)] public float hitNormalizedTime = 0.35f;

    public int attackDamage = 1;
    public int skillDamage = 2;

    public Vector3 hitboxHalfExtents = new Vector3(0.7f, 0.6f, 0.8f);
    public float hitboxForwardOffset = 1.1f;

    public LayerMask enemyMask;
    public LayerMask environmentMask;

    public float attackWallRecoilSpeed = 14f;

    [Range(-1f, 0f)] public float downAttackThreshold = -0.35f;
    public float downAttackBounceVelocity = 12f;

    [Header("Dice Gauge")]
    public int diceGaugeMax = 10;
    public int diceGainOnAttackHit = 1;

    [Header("Ledge Assist")]
    public float ledgeAssistDuration = 0.15f;
    public float ledgeSensorRadius = 0.18f;
    public float ledgeProbeUp = 0.6f;
    public float ledgeProbeForward = 0.15f;
    public float ledgeProbeDown = 1.8f;
    public float ledgeSnapInset = 0.25f;
    public float ledgeSnapExtraY = 0.02f;

    [Header("Hit Feedback")]
    public float hitStopDuration = 0.2f;
    [Range(0f, 1f)] public float darkenAlpha = 0.35f;
    public float shakeDuration = 0.2f;
    public float shakeAmplitude = 0.25f;
    public float shakeFrequency = 25f;

    [Header("Camera")]
    [Range(0f, 1f)] public float lookAheadPercent = 0.15f;
    [Range(0f, 1f)] public float cliffDownPercent = 0.05f;
    [Range(0f, 1f)] public float lookDownPercent = 0.40f;
    public float cameraFollowSpeed = 40f;
    public float cameraSmoothTime = 0.12f;
    public float lookDownHoldTime = 1f;

    [Header("Camera Cliff Probe")]
    public float cliffProbeForward = 0.8f;
    public float cliffProbeDown = 4f;
}