using UnityEngine;

public readonly struct DamagePayload
{
    public readonly int Amount;
    public readonly GameObject Source;

    public DamagePayload(int amount, GameObject Attacker)
    {
        Amount = amount;
        Source = Attacker;
    }
}

public interface IDamageable
{
    public void ApplyDamage(DamagePayload payload);
}
