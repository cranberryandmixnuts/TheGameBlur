using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class BossFireballProjectile : MonoBehaviour
{
    [SerializeField] float lifeTime = 6f;

    int damage;
    GameObject owner;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void Init(int damage, GameObject owner, Vector3 velocity)
    {
        this.damage = damage;
        this.owner = owner;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = velocity;
        }

        Destroy(gameObject, Mathf.Max(0.1f, lifeTime));
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player != null)
        {
            player.ApplyDamage(new DamagePayload(damage, owner));
            Destroy(gameObject);
        }
    }
}
