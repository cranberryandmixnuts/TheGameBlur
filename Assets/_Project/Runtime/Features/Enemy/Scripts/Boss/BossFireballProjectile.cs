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
        this.damage = damage;
        this.owner = owner;

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
        {
            Destroy(gameObject);
        }
    }
}
