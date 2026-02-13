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
            if (rng != null) finalDamage = rng.ApplyCritToOutgoingDamage(finalDamage);
        }
        this.damage = finalDamage;

        rb.linearVelocity = velocity;

        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (owner != null && other.transform.IsChildOf(owner.transform))
            return;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
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
