using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player/Ultimates/SliceAndDice", fileName = "Ultimate_SliceAndDice")]
public sealed class BasicPlayerUltimate : PlayerUltimate
{
    [Header("Hits")]
    [SerializeField] private int hitsPerPip = 2;
    [SerializeField] private int damagePerHit = 8;
    [SerializeField] private float hitInterval = 0.08f;
    [SerializeField] private float endLag = 0.0f;

    [Header("Box (XYZ Length + Offset)")]
    [SerializeField] private Vector3 boxSizeBase = new Vector3(3.0f, 2.0f, 3.0f);
    [SerializeField] private Vector3 boxSizePerPip = new Vector3(0.2f, 0.1f, 0.0f);

    [SerializeField] private Vector3 boxOffsetBase = new Vector3(1.2f, 0.5f, 0.0f);
    [SerializeField] private Vector3 boxOffsetPerPip = new Vector3(0.08f, 0.03f, 0.0f);

    public override float GetLockDuration(Player player, int directionSign, Vector3 mouseWorld)
    {
        int diceSum = player.Stats.DiceA + player.Stats.DiceB;
        if (diceSum < 1) diceSum = 1;

        int totalHits = diceSum * hitsPerPip;
        if (totalHits < 1) totalHits = 1;

        float d = totalHits * hitInterval + endLag;
        if (d < 0f) d = 0f;

        return d;
    }

    public override void Execute(Player player, int directionSign, Vector3 mouseWorld)
    {
        int diceSum = player.Stats.DiceA + player.Stats.DiceB;
        if (diceSum < 1) diceSum = 1;

        int totalHits = diceSum * hitsPerPip;
        if (totalHits < 1) totalHits = 1;

        Vector3 size = boxSizeBase + boxSizePerPip * diceSum;

        Vector3 offset = boxOffsetBase + boxOffsetPerPip * diceSum;
        offset.x *= directionSign;

        Vector3 p = player.transform.position;
        p.z = player.Settings.planeZ;

        Vector3 center = p + new Vector3(offset.x, offset.y, 0f);
        center.z = player.Settings.planeZ;

        float duration = GetLockDuration(player, directionSign, mouseWorld);

        PlayerAttackRangeIndicator indicator = player.Combat.AttackRangeIndicator;
        if (indicator != null) indicator.ShowUltimateBox(center, size, duration);

        player.StartCoroutine(RunHits(player, center, size, totalHits));
    }

    private IEnumerator RunHits(Player player, Vector3 center, Vector3 size, int totalHits)
    {
        Collider[] buffer = new Collider[96];
        HashSet<IDamageable> hitSet = new HashSet<IDamageable>();
        LayerMask mask = player.Settings.attackMask;

        float interval = hitInterval;
        if (interval < 0f) interval = 0f;

        Vector3 halfExtents = size * 0.5f;

        for (int i = 0; i < totalHits; i++)
        {
            ApplyHit(player, center, halfExtents, mask, buffer, hitSet);

            if (i != totalHits - 1 && interval > 0f)
                yield return new WaitForSeconds(interval);
        }
    }

    private void ApplyHit(Player player, Vector3 center, Vector3 halfExtents, LayerMask mask, Collider[] buffer, HashSet<IDamageable> hitSet)
    {
        hitSet.Clear();

        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, buffer, Quaternion.identity, mask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider c = buffer[i];
            if (c == null) continue;
            if (c.transform.IsChildOf(player.transform)) continue;

            IDamageable d = c.GetComponentInParent<IDamageable>();
            if (d == null) continue;

            if (hitSet.Add(d))
                d.ApplyDamage(new DamagePayload(damagePerHit, player.gameObject));
        }
    }
}