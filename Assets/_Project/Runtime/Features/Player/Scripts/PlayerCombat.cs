using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerDiceGauge))]
public sealed class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Transform hitOrigin;

    [Header("Debug")]
    [SerializeField] private bool debugShowHitbox = true;
    [SerializeField] private float debugPersistSeconds = 0.15f;
    [SerializeField] private AttackHitboxVisualizer hitboxVisualizer;

    private PlayerMotor motor;
    private PlayerDiceGauge diceGauge;

    private PlayerSettings settings;

    private bool active;
    private bool isSkill;
    private float elapsed;
    private float duration;

    private bool hitFired;
    private bool diceGainedThisAction;

    private Vector3 aimDir;

    private readonly Collider[] overlap = new Collider[32];
    private readonly HashSet<EnemyScript> hitEnemies = new();

    public bool IsActionActive => active;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        diceGauge = GetComponent<PlayerDiceGauge>();

        if (hitboxVisualizer == null)
        {
            GameObject go = new("AttackHitboxDebug");
            go.transform.SetParent(transform, false);
            hitboxVisualizer = go.AddComponent<AttackHitboxVisualizer>();
        }
    }

    private void Update()
    {
        if (!active)
            return;

        elapsed += Time.deltaTime;

        float normalized = duration > 0f ? elapsed / duration : 1f;

        if (!hitFired && normalized >= settings.hitNormalizedTime)
        {
            hitFired = true;
            PerformHit();
        }

        if (elapsed >= duration)
            active = false;
    }

    public void BeginAttack(PlayerSettings settings, Vector3 aimDir, bool isSkill)
    {
        this.settings = settings;
        this.isSkill = isSkill;

        active = true;
        elapsed = 0f;
        duration = isSkill ? settings.skillAnimTime : settings.attackAnimTime;

        hitFired = false;
        diceGainedThisAction = false;

        this.aimDir = aimDir;
        this.aimDir.z = 0f;

        hitEnemies.Clear();
    }

    private void PerformHit()
    {
        Vector3 origin = hitOrigin.position;

        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

        Vector3 center = origin + aimDir * settings.hitboxForwardOffset;
        center.z = origin.z;

        if (debugShowHitbox)
            hitboxVisualizer.Show(center, settings.hitboxHalfExtents, rot, debugPersistSeconds);

        int envCount = Physics.OverlapBoxNonAlloc(center, settings.hitboxHalfExtents, overlap, rot, settings.environmentMask, QueryTriggerInteraction.Ignore);
        if (envCount > 0)
            motor.ApplyWallRecoil(settings, aimDir);

        int enemyCount = Physics.OverlapBoxNonAlloc(center, settings.hitboxHalfExtents, overlap, rot, settings.enemyMask, QueryTriggerInteraction.Ignore);

        bool hitAnyEnemy = false;

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyScript enemy = overlap[i].GetComponentInParent<EnemyScript>();
            if (enemy == null)
                continue;

            if (!hitEnemies.Add(enemy))
                continue;

            int dmg = isSkill ? settings.skillDamage : settings.attackDamage;
            enemy.TakeDamage(dmg);

            hitAnyEnemy = true;

            if (!motor.IsGrounded)
                motor.ResetAirDash();

            if (!motor.IsGrounded && aimDir.y <= settings.downAttackThreshold)
                motor.ApplyDownAttackBounce(settings);
        }

        if (hitAnyEnemy && !isSkill && !diceGainedThisAction)
        {
            diceGainedThisAction = true;
            diceGauge.Add(settings.diceGainOnAttackHit);
        }
    }
}