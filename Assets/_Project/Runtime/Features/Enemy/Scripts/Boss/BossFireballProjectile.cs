using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class BossFireballProjectile : MonoBehaviour
{
    public float lifeTime = 4f;

    int damage;
    GameObject owner;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void Init(int damage, GameObject owner, Vector3 velocity)
    {
        this.owner = owner;

        int finalDamage = damage;
        if (owner != null)
        {
            var rng = owner.GetComponent<EnemyCombatRng>();
            if (rng != null)
                finalDamage = rng.ApplyCritToOutgoingDamage(finalDamage);
        }
        this.damage = finalDamage;

        rb.linearVelocity = velocity;

        // 위/아래 기울기 계산용 각도
        float pitchAngle = Mathf.Atan2(velocity.y, Mathf.Abs(velocity.x)) * Mathf.Rad2Deg;

        // 좌우는 기존처럼 Y축으로 처리
        if (velocity.x < 0f)
        {
            transform.rotation = Quaternion.Euler(pitchAngle, 90f, 0f);
        }
        else if (velocity.x > 0f)
        {
            transform.rotation = Quaternion.Euler(-pitchAngle, -90f, 0f);
        }

        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (owner != null && other.transform.IsChildOf(owner.transform))
            return;

        IDamageable player = other.GetComponent<IDamageable>();
        if (player != null)
        {
            player.ApplyDamage(new DamagePayload(damage, owner != null ? owner : gameObject));
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }
}