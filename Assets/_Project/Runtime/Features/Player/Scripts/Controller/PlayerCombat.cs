using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerCombat : MonoBehaviour
{
    public bool IsSkillOrUltimateActive => skillLockRemaining > 0f || ultimateLockRemaining > 0f;
    public PlayerSkill EquippedSkill => equippedSkill;
    public PlayerUltimate EquippedUltimate => equippedUltimate;

    public float UltimateGaugeMax
    {
        get
        {
            if (equippedUltimate != null) return equippedUltimate.GaugeMax;
            return settings.DefaultUltimateGaugeMax;
        }
    }

    private PlayerSettings settings;
    private PlayerStats stats;
    private PlayerMovement movement;
    private InputManager input;

    private PlayerSkill equippedSkill;
    private PlayerUltimate equippedUltimate;

    private float basicAttackCooldownRemaining;
    private float skillLockRemaining;
    private float ultimateLockRemaining;

    private readonly Collider[] hitBuffer = new Collider[48];
    private readonly HashSet<IDamageable> hitSet = new HashSet<IDamageable>();

    private void Start()
    {
        Player player = Player.Instance;

        settings = player.Settings;
        stats = player.Stats;
        movement = player.Movement;
        input = player.Input;

        equippedSkill = settings.StartingSkill;
        equippedUltimate = settings.StartingUltimate;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (basicAttackCooldownRemaining > 0f) basicAttackCooldownRemaining -= dt;
        if (skillLockRemaining > 0f) skillLockRemaining -= dt;
        if (ultimateLockRemaining > 0f) ultimateLockRemaining -= dt;

        if (movement.IsDashing) return;

        if (input.AttackDown) TryBasicAttack();
        if (input.SkillDown) TryUseSkill();
        if (input.DiceSkillDown) TryUseUltimate();
    }

    public void CancelForDash()
    {
        basicAttackCooldownRemaining = 0f;
    }

    public void EquipSkill(PlayerSkill skill)
    {
        if (skill != null && !IsSkillUnlocked(skill)) return;
        equippedSkill = skill;
    }

    public void EquipUltimate(PlayerUltimate ultimate)
    {
        if (ultimate != null && !IsUltimateUnlocked(ultimate)) return;
        equippedUltimate = ultimate;

        float max = UltimateGaugeMax;
        if (stats.DiceGauge > max) stats.AddDiceGauge(max - stats.DiceGauge);
    }

    private void TryBasicAttack()
    {
        if (IsSkillOrUltimateActive) return;
        if (basicAttackCooldownRemaining > 0f) return;

        if (!TryGetMouseWorldOnPlane(out Vector3 mouseWorld))
            mouseWorld = transform.position + Vector3.right * movement.FacingSign;

        if (movement.IsGrounded) GroundAttack(mouseWorld);
        else AirAttack(mouseWorld);

        basicAttackCooldownRemaining = settings.BasicAttackCooldown;
        stats.NotifyCombatActivity();
    }

    private void GroundAttack(Vector3 mouseWorld)
    {
        Vector3 p = transform.position;

        Vector2 d = new Vector2(mouseWorld.x - p.x, mouseWorld.y - p.y);
        if (d.sqrMagnitude < 0.0001f) d = new Vector2(movement.FacingSign, 0f);
        d.Normalize();

        Vector3 center = new Vector3(p.x + d.x * settings.GroundAttackReach, p.y + d.y * settings.GroundAttackReach, settings.PlaneZ);

        int count = Physics.OverlapSphereNonAlloc(center, settings.GroundAttackRadius, hitBuffer, settings.AttackMask, QueryTriggerInteraction.Ignore);
        DealDamageFromHits(count, settings.GroundAttackDamage);
    }

    private void AirAttack(Vector3 mouseWorld)
    {
        Vector3 p = transform.position;

        int sign = mouseWorld.x >= p.x ? 1 : -1;

        int count = Physics.OverlapSphereNonAlloc(new Vector3(p.x, p.y, settings.PlaneZ), settings.AirAttackRadius, hitBuffer, settings.AttackMask, QueryTriggerInteraction.Ignore);

        hitSet.Clear();

        Vector2 forward = new Vector2(sign, 0f);
        float halfAngle = settings.AirAttackHalfAngleDeg;

        for (int i = 0; i < count; i++)
        {
            Collider c = hitBuffer[i];
            if (c == null) continue;
            if (c.transform.IsChildOf(transform)) continue;

            Vector3 cp = c.bounds.center;
            Vector2 v = new Vector2(cp.x - p.x, cp.y - p.y);

            if (v.sqrMagnitude < 0.0001f) continue;
            if (v.x * sign <= 0f) continue;

            float ang = Vector2.Angle(forward, v);
            if (ang > halfAngle) continue;

            IDamageable d = c.GetComponentInParent<IDamageable>();
            if (d == null) continue;

            if (hitSet.Add(d))
                d.ApplyDamage(new DamagePayload(settings.AirAttackDamage, gameObject));
        }
    }

    private void DealDamageFromHits(int count, int amount)
    {
        hitSet.Clear();

        for (int i = 0; i < count; i++)
        {
            Collider c = hitBuffer[i];
            if (c == null) continue;
            if (c.transform.IsChildOf(transform)) continue;

            IDamageable d = c.GetComponentInParent<IDamageable>();
            if (d == null) continue;

            if (hitSet.Add(d))
                d.ApplyDamage(new DamagePayload(amount, gameObject));
        }
    }

    private void TryUseSkill()
    {
        if (IsSkillOrUltimateActive) return;
        if (equippedSkill == null) return;
        if (!IsSkillUnlocked(equippedSkill)) return;

        if (!TryGetMouseWorldOnPlane(out Vector3 mouseWorld))
            mouseWorld = transform.position + Vector3.right * movement.FacingSign;

        int sign = mouseWorld.x >= transform.position.x ? 1 : -1;

        if (!stats.TrySpendMana(equippedSkill.ManaCost)) return;

        equippedSkill.Execute(Player.Instance, sign, mouseWorld);
        skillLockRemaining = equippedSkill.LockDuration;

        stats.NotifyCombatActivity();
    }

    private void TryUseUltimate()
    {
        if (IsSkillOrUltimateActive) return;
        if (equippedUltimate == null) return;
        if (!IsUltimateUnlocked(equippedUltimate)) return;

        float max = UltimateGaugeMax;
        if (max <= 0f) return;
        if (!stats.IsDiceGaugeFull()) return;

        if (!TryGetMouseWorldOnPlane(out Vector3 mouseWorld))
            mouseWorld = transform.position + Vector3.right * movement.FacingSign;

        int sign = mouseWorld.x >= transform.position.x ? 1 : -1;

        stats.ConsumeAllDiceGauge();
        equippedUltimate.Execute(Player.Instance, sign, mouseWorld);
        ultimateLockRemaining = equippedUltimate.LockDuration;

        stats.NotifyCombatActivity();
    }

    private bool IsSkillUnlocked(PlayerSkill skill)
    {
        PlayerSkill[] list = settings.UnlockedSkills;
        for (int i = 0; i < list.Length; i++)
            if (list[i] == skill) return true;

        return false;
    }

    private bool IsUltimateUnlocked(PlayerUltimate ultimate)
    {
        PlayerUltimate[] list = settings.UnlockedUltimates;
        for (int i = 0; i < list.Length; i++)
            if (list[i] == ultimate) return true;

        return false;
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
}