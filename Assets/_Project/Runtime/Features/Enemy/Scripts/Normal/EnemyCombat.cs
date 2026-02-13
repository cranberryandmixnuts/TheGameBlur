using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyScript))]
public sealed class EnemyCombat : MonoBehaviour
{
    [SerializeField] private Transform hitOrigin;

    [Header("Debug")]
    [SerializeField] private bool debugShowHitbox = true;
    [SerializeField] private float debugPersistSeconds = 0.15f;
    [SerializeField] private AttackHitboxVisualizer hitboxVisualizer;

    private EnemyScript enemy;

    private bool active;
    private float elapsed;
    private float duration;
    private bool hitFired;

    private Vector3 aimDir;

    private readonly Collider[] overlap = new Collider[32];
    private readonly HashSet<PlayerStats> hitPlayers = new HashSet<PlayerStats>();

    public bool IsActionActive => active;

    private void Awake()
    {
        enemy = GetComponent<EnemyScript>();

        if (hitOrigin == null)
            hitOrigin = transform;

        if (hitboxVisualizer == null)
        {
            GameObject go = new GameObject("EnemyAttackHitboxDebug");
            go.transform.SetParent(transform, false);
            hitboxVisualizer = go.AddComponent<AttackHitboxVisualizer>();
        }
    }

    private void Update()
    {
        if (!active) return;

        elapsed += Time.deltaTime;

        float normalized = duration > 0f ? elapsed / duration : 1f;

        if (!hitFired && normalized >= enemy.data.hitNormalizedTime)
        {
            hitFired = true;
            PerformHit();
        }

        if (elapsed >= duration)
            active = false;
    }

    public void BeginAttack()
    {
        if (active) return;

        active = true;
        elapsed = 0f;

        duration = Mathf.Max(0.01f, enemy.data.attackAnimTime);
        hitFired = false;

        hitPlayers.Clear();

        enemy.BeginAttackLock(duration);

        enemy.PlayAttackAnimation();

        aimDir = (enemy.FacingDir >= 0) ? Vector3.right : Vector3.left;
        aimDir.z = 0f;
    }


    private void PerformHit()
    {
        Vector3 origin = hitOrigin.position;

        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

        Vector3 center = origin + aimDir * enemy.data.hitboxForwardOffset;
        center.z = origin.z;

        if (debugShowHitbox)
            hitboxVisualizer.Show(center, enemy.data.hitboxHalfExtents, rot, debugPersistSeconds);

        int count = Physics.OverlapBoxNonAlloc(
            center,
            enemy.data.hitboxHalfExtents,
            overlap,
            rot,
            enemy.data.playerMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < count; i++)
        {
            PlayerStats player = overlap[i].GetComponentInParent<PlayerStats>();
            if (player == null) continue;
            if (!hitPlayers.Add(player)) continue;

            player.ApplyDamage(new DamagePayload(enemy.data.damage, gameObject));
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (hitOrigin == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(hitOrigin.position, 0.05f);
    }
}
