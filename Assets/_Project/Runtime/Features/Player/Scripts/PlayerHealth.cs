using System;
using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 10;

    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;

    public event Action<DamagePayload> OnDamaged;

    private void Awake()
    {
        CurrentHP = maxHP;
    }

    public void ApplyDamage(DamagePayload payload)
    {
        if (payload.Amount <= 0)
            return;

        CurrentHP -= payload.Amount;
        if (CurrentHP < 0)
        {
            CurrentHP = 0;
            Destroy(gameObject);
        }

        OnDamaged?.Invoke(payload);
    }
}
