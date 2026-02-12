using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Settings", fileName = "PlayerSettings")]
public sealed class PlayerSettings : ScriptableObject
{
    [Header("Plane")]
    [SerializeField] private float planeZ = 0f;

    [Header("Move - Common")]
    [SerializeField] private float moveDeadZone = 0.01f;
    [SerializeField] private float baseMoveSpeed = 6f;
    [SerializeField] private float runSpeedMultiplier = 1.5f;
    [SerializeField] private float backwardMoveSpeedMultiplier = 0.7f;

    [Header("Move - Ground")]
    [SerializeField] private float groundStartSpeedMultiplier = 0.3f;
    [SerializeField] private float groundAccelTimeToMax = 0.05f;

    [Header("Move - Air")]
    [SerializeField] private float airAccelTimeToMax = 0.18f;
    [SerializeField] private float airDecel = 18f;

    [Header("Facing")]
    [SerializeField] private float mouseFacingDeadZone = 0.02f;

    [Header("Jump")]
    [SerializeField] private float gravity = -35f;
    [SerializeField] private float maxFallSpeed = 45f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float maxJumpHoldTime = 0.25f;
    [SerializeField] private float maxJumpForce = 70f;
    [SerializeField] private AnimationCurve jumpForceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("Dash")]
    [SerializeField] private float dashCooldown = 0.8f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashDistance = 5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private float groundedMinUpVelocity = 0.05f;

    [Header("Stats - Base")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int maxMp = 100;

    [Header("Attack - Common")]
    [SerializeField] private LayerMask attackMask = ~0;
    [SerializeField] private float basicAttackCooldown = 0.08f;

    [Header("Attack - Ground")]
    [SerializeField] private int groundAttackDamage = 10;
    [SerializeField] private float groundAttackReach = 1.6f;
    [SerializeField] private float groundAttackRadius = 1.2f;

    [Header("Attack - Air")]
    [SerializeField] private int airAttackDamage = 10;
    [SerializeField] private float airAttackRadius = 2.2f;
    [SerializeField] private float airAttackHalfAngleDeg = 75f;

    [Header("Dice System")]
    [SerializeField] private float diceRollIntervalMin = 7f;
    [SerializeField] private float diceRollIntervalMax = 10f;
    [SerializeField] private float diceGaugeGainPerRoll = 30f;
    [SerializeField] private float defaultUltimateGaugeMax = 100f;

    [Header("Loadout")]
    [SerializeField] private PlayerSkill startingSkill;
    [SerializeField] private PlayerUltimate startingUltimate;

    [Header("Unlocks")]
    [SerializeField] private PlayerSkill[] unlockedSkills;
    [SerializeField] private PlayerUltimate[] unlockedUltimates;

    public float PlaneZ => planeZ;

    public float MoveDeadZone => moveDeadZone;
    public float BaseMoveSpeed => baseMoveSpeed;
    public float RunSpeedMultiplier => runSpeedMultiplier;
    public float BackwardMoveSpeedMultiplier => backwardMoveSpeedMultiplier;

    public float GroundStartSpeedMultiplier => groundStartSpeedMultiplier;
    public float GroundAccelTimeToMax => groundAccelTimeToMax;

    public float AirAccelTimeToMax => airAccelTimeToMax;
    public float AirDecel => airDecel;

    public float MouseFacingDeadZone => mouseFacingDeadZone;

    public float Gravity => gravity;
    public float MaxFallSpeed => maxFallSpeed;
    public float CoyoteTime => coyoteTime;
    public float MaxJumpHoldTime => maxJumpHoldTime;
    public float MaxJumpForce => maxJumpForce;
    public AnimationCurve JumpForceCurve => jumpForceCurve;

    public float DashCooldown => dashCooldown;
    public float DashDuration => dashDuration;
    public float DashDistance => dashDistance;

    public LayerMask GroundMask => groundMask;
    public float GroundCheckDistance => groundCheckDistance;
    public float GroundedMinUpVelocity => groundedMinUpVelocity;

    public int MaxHp => maxHp;
    public int MaxMp => maxMp;

    public LayerMask AttackMask => attackMask;
    public float BasicAttackCooldown => basicAttackCooldown;

    public int GroundAttackDamage => groundAttackDamage;
    public float GroundAttackReach => groundAttackReach;
    public float GroundAttackRadius => groundAttackRadius;

    public int AirAttackDamage => airAttackDamage;
    public float AirAttackRadius => airAttackRadius;
    public float AirAttackHalfAngleDeg => airAttackHalfAngleDeg;

    public float DiceRollIntervalMin => diceRollIntervalMin;
    public float DiceRollIntervalMax => diceRollIntervalMax;
    public float DiceGaugeGainPerRoll => diceGaugeGainPerRoll;
    public float DefaultUltimateGaugeMax => defaultUltimateGaugeMax;

    public PlayerSkill StartingSkill => startingSkill;
    public PlayerUltimate StartingUltimate => startingUltimate;

    public PlayerSkill[] UnlockedSkills => unlockedSkills;
    public PlayerUltimate[] UnlockedUltimates => unlockedUltimates;
}