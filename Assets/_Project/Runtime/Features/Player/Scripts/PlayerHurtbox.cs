using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerMotor))]
public sealed class PlayerHurtbox : MonoBehaviour, IDamageable
{
    private PlayerHealth health;
    private PlayerMotor motor;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        motor = GetComponent<PlayerMotor>();
    }

    public void ApplyDamage(DamagePayload payload)
    {
        if (motor.IsInvincible)
            return;

        health.ApplyDamage(payload);
    }
}
