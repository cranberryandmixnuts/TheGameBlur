using UnityEngine;

public abstract class PlayerUltimate : ScriptableObject
{
    [SerializeField] private float gaugeMax = 100f;
    [SerializeField] private float lockDuration = 0.4f;

    public float GaugeMax => gaugeMax;
    public float LockDuration => lockDuration;

    public abstract void Execute(Player player, int directionSign, Vector3 mouseWorld);
}