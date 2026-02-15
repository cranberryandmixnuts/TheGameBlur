using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerCombat : MonoBehaviour
{
    public enum AnimRequest
    {
        Attack,
        AirAttack,
        Technology
    }

    public event Action<AnimRequest> AnimationRequested;
    public event Action OnAttacked;

    [SerializeField] private PlayerAttackRangeIndicator attackRangeIndicator;

    public PlayerAttackRangeIndicator AttackRangeIndicator => attackRangeIndicator;

    public bool IsSkillOrUltimateActive => skillLockRemaining > 0f || ultimateLockRemaining > 0f;
    public bool IsUltimateActive => ultimateLockRemaining > 0f;

    public float SkillLockRemaining => skillLockRemaining;
    public float SkillLockDuration => equippedSkill != null ? equippedSkill.LockDuration : 0f;

    public float SkillCooldownRemaining => skillCooldownRemaining;
    public float SkillCooldownDuration => equippedSkill != null ? equippedSkill.Cooldown : 0f;

    public PlayerSkill EquippedSkill => equippedSkill;
    public PlayerUltimate EquippedUltimate => equippedUltimate;

    public float UltimateGaugeMax => equippedUltimate != null && equippedUltimate.DiceEnabled ? equippedUltimate.GaugeMax : 0f;

    private Player player;
    private PlayerSettings settings;
    private PlayerStats stats;
    private PlayerMovement movement;
    private InputManager input;

    private PlayerSkill equippedSkill;
    private PlayerUltimate equippedUltimate;

    private float basicAttackCooldownRemaining;
    private float skillLockRemaining;
    private float ultimateLockRemaining;
    private float skillCooldownRemaining;

    private bool ultimateInvincibleApplied;

    private bool lastGrounded;
    private bool usedAirBasicAttackThisAirtime;

    private readonly Collider[] hitBuffer = new Collider[48];
    private readonly HashSet<IDamageable> hitSet = new HashSet<IDamageable>();

    private readonly HashSet<PlayerSkill> unlockedSkills = new HashSet<PlayerSkill>();
    private readonly HashSet<PlayerUltimate> unlockedUltimates = new HashSet<PlayerUltimate>();

    private void Reset()
    {
        attackRangeIndicator = GetComponent<PlayerAttackRangeIndicator>();
    }

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        settings = player.Settings;
        stats = player.Stats;
        movement = player.Movement;
        input = player.Input;

        if (attackRangeIndicator == null) attackRangeIndicator = GetComponent<PlayerAttackRangeIndicator>();

        RebuildUnlockCache();

        equippedSkill = settings.startingSkill;
        equippedUltimate = settings.startingUltimate;

        if (equippedSkill != null) unlockedSkills.Add(equippedSkill);
        if (equippedUltimate != null) unlockedUltimates.Add(equippedUltimate);

        lastGrounded = movement.IsGrounded;
        usedAirBasicAttackThisAirtime = false;
    }

    private void Update()
    {
        if (!player.Stats.IsActive) return;

        float dt = Time.deltaTime;

        if (basicAttackCooldownRemaining > 0f) basicAttackCooldownRemaining -= dt;
        if (skillLockRemaining > 0f) skillLockRemaining -= dt;

        if (ultimateLockRemaining > 0f)
        {
            ultimateLockRemaining -= dt;
            if (ultimateLockRemaining <= 0f && ultimateInvincibleApplied)
            {
                ultimateInvincibleApplied = false;
                stats.SetInvincible(false);
            }
        }

        if (skillCooldownRemaining > 0f) skillCooldownRemaining -= dt;

        bool grounded = movement.IsGrounded;
        if (grounded && !lastGrounded)
            usedAirBasicAttackThisAirtime = false;

        lastGrounded = grounded;

        if (player.IsSitting) return;
        if (movement.IsDashing) return;

        if (input.AttackDown) TryBasicAttack();
        if (input.SkillDown) TryUseSkill();
        if (input.DiceSkillDown) TryUseUltimate();
    }

    public void ResetSkillCooldown()
    {
        basicAttackCooldownRemaining = 0f;
        skillLockRemaining = 0f;
        ultimateLockRemaining = 0f;
        skillCooldownRemaining = 0f;

        movement.CancelBasicAttackRunLock();

        if (ultimateInvincibleApplied)
        {
            ultimateInvincibleApplied = false;
            stats.SetInvincible(false);
        }
    }

    public void RebuildUnlockCache()
    {
        unlockedSkills.Clear();
        unlockedUltimates.Clear();

        PlayerSettings.SkillUnlockEntry[] s = settings.skills;
        for (int i = 0; i < s.Length; i++)
        {
            if (!s[i].unlocked) continue;
            if (s[i].skill == null) continue;
            unlockedSkills.Add(s[i].skill);
        }

        PlayerSettings.UltimateUnlockEntry[] u = settings.ultimates;
        for (int i = 0; i < u.Length; i++)
        {
            if (!u[i].unlocked) continue;
            if (u[i].ultimate == null) continue;
            unlockedUltimates.Add(u[i].ultimate);
        }
    }

    public bool IsSkillUnlocked(PlayerSkill skill)
    {
        return skill != null && (skill.IsAlwaysUnlocked || unlockedSkills.Contains(skill));
    }

    public bool IsUltimateUnlocked(PlayerUltimate ultimate)
    {
        return ultimate != null && (ultimate.IsAlwaysUnlocked || unlockedUltimates.Contains(ultimate));
    }

    public void EquipSkill(PlayerSkill skill)
    {
        if (skill != null && !IsSkillUnlocked(skill)) return;

        equippedSkill = skill;
        skillCooldownRemaining = 0f;
    }

    public void EquipUltimate(PlayerUltimate ultimate)
    {
        if (ultimate != null && !IsUltimateUnlocked(ultimate)) return;

        equippedUltimate = ultimate;

        float max = UltimateGaugeMax;
        if (max <= 0f) stats.ConsumeAllDiceGauge();
        else if (stats.DiceGauge > max) stats.AddDiceGauge(max - stats.DiceGauge);
    }

    public void CancelForDash()
    {
        basicAttackCooldownRemaining = 0f;
        movement.CancelBasicAttackRunLock();
    }

    private void TryBasicAttack()
    {
        if (IsSkillOrUltimateActive) return;
        if (basicAttackCooldownRemaining > 0f) return;

        if (movement.IsGrounded)
        {
            if (!TryGetMouseWorldOnPlane(out Vector3 mouseWorld))
                mouseWorld = transform.position + Vector3.right * movement.FacingSign;

            GroundAttack(mouseWorld);

            basicAttackCooldownRemaining = settings.basicAttackCooldown;
            movement.NotifyBasicAttackStarted(settings.basicAttackCooldown);
            return;
        }

        if (usedAirBasicAttackThisAirtime) return;

        bool hitAny = AirAttack();

        usedAirBasicAttackThisAirtime = true;
        basicAttackCooldownRemaining = settings.basicAttackCooldown;
        movement.NotifyBasicAttackStarted(settings.basicAttackCooldown);

        if (hitAny)
        {
            usedAirBasicAttackThisAirtime = false;
            movement.RefreshAirDashUsage();
            movement.ApplyAirPogoBounce();
        }
    }

    private void GroundAttack(Vector3 mouseWorld)
    {
        Vector3 p = transform.position;

        Vector2 d = new Vector2(mouseWorld.x - p.x, mouseWorld.y - p.y);
        if (d.sqrMagnitude < 0.0001f) d = new Vector2(movement.FacingSign, 0f);
        d.Normalize();

        Vector3 center = new Vector3(p.x + d.x * settings.groundAttackReach, p.y + d.y * settings.groundAttackReach, settings.planeZ);

        if (attackRangeIndicator != null)
            attackRangeIndicator.ShowGroundCircle(center, settings.groundAttackRadius);

        int count = Physics.OverlapSphereNonAlloc(center, settings.groundAttackRadius, hitBuffer, settings.attackMask, QueryTriggerInteraction.Ignore);
        DealDamageFromHits(count, settings.groundAttackDamage);

        AnimationRequested?.Invoke(AnimRequest.Attack);
    }

    private bool AirAttack()
    {
        Vector3 p = transform.position;
        Vector3 center = new Vector3(p.x, p.y, settings.planeZ);

        if (attackRangeIndicator != null)
            attackRangeIndicator.ShowAirCircle(center, settings.airAttackRadius);

        int count = Physics.OverlapSphereNonAlloc(center, settings.airAttackRadius, hitBuffer, settings.attackMask, QueryTriggerInteraction.Ignore);
        bool hitAny = DealDamageFromHits(count, settings.airAttackDamage);

        AnimationRequested?.Invoke(AnimRequest.AirAttack);

        return hitAny;
    }

    private bool DealDamageFromHits(int count, int amount)
    {
        float critChance = DiceChanceTable.GetPlayerChance(stats.DiceValue);
        if (critChance > 0f && UnityEngine.Random.value < critChance) amount *= 2;

        hitSet.Clear();
        bool hitAny = false;

        for (int i = 0; i < count; i++)
        {
            Collider c = hitBuffer[i];
            if (c == null) continue;
            if (c.transform.IsChildOf(transform)) continue;

            IDamageable d2 = c.GetComponentInParent<IDamageable>();
            if (d2 == null) continue;

            if (hitSet.Add(d2))
            {
                d2.ApplyDamage(new DamagePayload(amount, gameObject));
                OnAttacked?.Invoke();
                hitAny = true;
            }
        }

        return hitAny;
    }

    private void TryUseSkill()
    {
        if (IsSkillOrUltimateActive) return;
        if (equippedSkill == null) return;
        if (!equippedSkill.IsEquipped) return;
        if (skillCooldownRemaining > 0f) return;

        if (!TryGetMouseWorldOnPlane(out Vector3 mouseWorld))
            mouseWorld = transform.position + Vector3.right * movement.FacingSign;

        int sign = mouseWorld.x >= transform.position.x ? 1 : -1;

        if (!stats.TrySpendMana(equippedSkill.ManaCost)) return;

        equippedSkill.Execute(player, sign, mouseWorld);

        skillLockRemaining = equippedSkill.LockDuration;
        skillCooldownRemaining = equippedSkill.Cooldown;

        AnimationRequested?.Invoke(AnimRequest.Technology);
    }

    private void TryUseUltimate()
    {
        if (IsSkillOrUltimateActive) return;
        if (equippedUltimate == null) return;
        if (!equippedUltimate.DiceEnabled) return;

        float max = UltimateGaugeMax;
        if (max <= 0f) return;
        if (!stats.IsDiceGaugeFull()) return;

        if (!TryGetMouseWorldOnPlane(out Vector3 mouseWorld))
            mouseWorld = transform.position + Vector3.right * movement.FacingSign;

        int sign = mouseWorld.x >= transform.position.x ? 1 : -1;

        stats.ConsumeAllDiceGauge();
        stats.RollDiceForUltimate();

        float lockDuration = equippedUltimate.GetLockDuration(player, sign, mouseWorld);
        if (lockDuration < 0f) lockDuration = 0f;

        ultimateLockRemaining = lockDuration;
        ultimateInvincibleApplied = true;
        stats.SetInvincible(true);

        equippedUltimate.Execute(player, sign, mouseWorld);
    }

    private bool TryGetMouseWorldOnPlane(out Vector3 world)
    {
        Camera cam = Camera.main;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, settings.planeZ));

        if (!plane.Raycast(ray, out float enter))
        {
            world = default;
            return false;
        }

        world = ray.GetPoint(enter);
        return true;
    }
}