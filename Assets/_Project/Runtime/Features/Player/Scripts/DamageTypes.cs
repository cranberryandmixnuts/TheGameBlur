using UnityEngine;

public readonly struct DamagePayload
{
    public readonly int Amount;
    public readonly GameObject Source;

    public DamagePayload(int amount, GameObject source)
    {
        Amount = amount;
        Source = source;
    }
}

public interface IDamageable
{
    public void ApplyDamage(DamagePayload payload);
}
