using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class FireballProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float remainingDistance;

    private int damage;
    private float hitRadius;

    private LayerMask enemyMask;
    private LayerMask worldMask;

    private GameObject source;
    private float planeZ;

    private readonly Collider[] hitBuffer = new Collider[32];
    private readonly HashSet<IDamageable> hitSet = new HashSet<IDamageable>();

    public void Initialize(Vector3 direction, float speed, float maxDistance, int damage, float hitRadius, LayerMask enemyMask, LayerMask worldMask, GameObject source, float planeZ, float size)
    {
        this.direction = direction;
        this.speed = speed;
        remainingDistance = maxDistance;

        this.damage = damage;
        this.hitRadius = hitRadius;

        this.enemyMask = enemyMask;
        this.worldMask = worldMask;

        this.source = source;
        this.planeZ = planeZ;

        Vector3 p = transform.position;
        p.z = planeZ;
        transform.position = p;

        transform.localScale = transform.localScale * size;

        AudioManager.Instance.PlaySFX("FireBall");
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        float step = speed * dt;

        if (step <= 0f) return;

        Vector3 p = transform.position;
        Vector3 next = p + direction * step;
        next.z = planeZ;

        if (Physics.SphereCast(p, hitRadius, direction, out RaycastHit hit, step, worldMask, QueryTriggerInteraction.Ignore))
        {
            transform.position = new Vector3(hit.point.x, hit.point.y, planeZ);
            Destroy(gameObject);
            return;
        }

        int count = Physics.OverlapSphereNonAlloc(next, hitRadius, hitBuffer, enemyMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider c = hitBuffer[i];
            if (c == null) continue;
            if (source != null && c.transform.IsChildOf(source.transform)) continue;

            IDamageable d = c.GetComponentInParent<IDamageable>();
            if (d == null) continue;

            if (hitSet.Add(d))
            {
                d.ApplyDamage(new DamagePayload(damage, source));
            }
        }

        transform.position = next;

        remainingDistance -= step;
        if (remainingDistance <= 0f) Destroy(gameObject);
    }
}