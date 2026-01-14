using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class DamageHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable d = other.GetComponentInParent<IDamageable>();
        if (d == null)
            return;

        d.ApplyDamage(new DamagePayload(damage, gameObject));
    }
}
