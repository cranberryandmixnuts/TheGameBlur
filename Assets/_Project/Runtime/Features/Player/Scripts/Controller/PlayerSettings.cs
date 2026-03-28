using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Settings", fileName = "PlayerSettings")]
public sealed class PlayerSettings : ScriptableObject
{
    [Serializable]
    public struct SkillUnlockEntry
    {
        public PlayerSkill skill;
        public bool unlocked;
    }

    [Serializable]
    public struct UltimateUnlockEntry
    {
        public PlayerUltimate ultimate;
        public bool unlocked;
    }

    [Header("Plane")]
    public float planeZ = 0f;

    [Header("Move - Common")]
    public float moveDeadZone = 0.01f;
    public float baseMoveSpeed = 6f;
    public float runSpeedMultiplier = 1.5f;
    public float backwardMoveSpeedMultiplier = 0.7f;

    [Header("Move - Ground")]
    public float groundStartSpeedMultiplier = 0.3f;
    public float groundAccelTimeToMax = 0.05f;

    [Header("Move - Air")]
    public float airAccelTimeToMax = 0.18f;
    public float airDecel = 18f;

    [Header("Facing")]
    public float mouseFacingDeadZone = 0.02f;
    public float facingTurnDuration = 0.16f;

    [Header("Jump / Gravity")]
    public float gravity = -35f;
    public float maxFallSpeed = 45f;
    public float coyoteTime = 0.1f;
    public float maxJumpHoldTime = 0.25f;
    public float maxJumpForce = 70f;
    public AnimationCurve jumpForceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("Dash")]
    public float dashCooldown = 0.8f;
    public float dashDuration = 0.2f;
    public float dashDistance = 5f;

    [Header("Ground (Trigger Sensor)")]
    public LayerMask groundMask = ~0;
    public float groundedMinUpVelocity = 0.05f;

    [Header("Stats - Base")]
    public int maxHp = 100;
    public int maxMp = 100;

    [Header("Attack - Common")]
    public LayerMask attackMask = ~0;
    public float basicAttackCooldown = 0.08f;

    [Header("Attack - Ground")]
    public int groundAttackDamage = 10;
    public float groundAttackReach = 1.6f;
    public float groundAttackRadius = 1.2f;

    [Header("Attack - Air")]
    public int airAttackDamage = 10;
    public float airAttackRadius = 2.6f;
    public float airPogoBounceVelocity = 12f;

    [Header("Attack Range Visual Debug")]
    public bool showAttackRangeOnAttack = true;
    public float attackRangeVisualDuration = 0.06f;
    public float attackRangeLineWidth = 0.03f;
    public int attackRangeSegments = 32;

    [Header("Dice System")]
    public float diceRollIntervalMin = 7f;
    public float diceRollIntervalMax = 10f;
    public float diceGaugeGainPerRoll = 30f;

    [Header("Loadout")]
    public PlayerSkill startingSkill;
    public PlayerUltimate startingUltimate;

    [Header("Unlock Table")]
    public SkillUnlockEntry[] skills;
    public UltimateUnlockEntry[] ultimates;

    [Header("Unlock - Consumable")]
    public bool potionUnlocked = true;

    [Header("UI - Dice Panel Animation")]
    public float uiDiceFrameTime = 0.08f;

    [Header("UI - Dice 3D Spin")]
    public float uiDiceLowerStopDelay = 0.6f;
    public float uiDiceUpperStopExtraDelay = 0.5f;
    public float uiDiceStopTweenTime = 0.08f;

    [Header("UI - Dice Face Rotations (Value 1..6 => Euler)")]
    public Vector3[] uiDiceFaceForwardEuler = new Vector3[6];

    [Header("UI - Dice Model Layer")]
    public int uiDiceModelLayer = 10;

    [Header("UI - Consumable")]
    public int uiPotionMaxUses = 3;
    public int uiPotionStartUses = 3;
    public int uiHpPotionHealAmount = 30;
    public int uiMpPotionHealAmount = 30;
    public float uiPotionHoldTime = 0.35f;

    [Header("UI - Skill")]
    public Color uiSkillGreyColor = new Color(0.35f, 0.35f, 0.35f, 1f);
}