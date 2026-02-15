using UnityEngine;

public abstract class PlayerUltimate : ScriptableObject
{
    [SerializeField] private GameObject dicePrefab;
    [SerializeField] private float gaugeMax = 100f;
    [SerializeField] private float lockDuration = 0.4f;

    public virtual bool IsAlwaysUnlocked => false;
    public virtual GameObject DicePrefab => dicePrefab;
    public virtual float GaugeMax => gaugeMax;
    public virtual float LockDuration => lockDuration;

    public virtual bool DiceEnabled => gaugeMax > 0f && dicePrefab != null;
    public virtual bool UseFixedDice => false;
    public virtual int FixedDiceA => 1;
    public virtual int FixedDiceB => 1;

    public virtual float GetLockDuration(Player player, int directionSign, Vector3 mouseWorld) => lockDuration;

    public abstract void Execute(Player player, int directionSign, Vector3 mouseWorld);
}