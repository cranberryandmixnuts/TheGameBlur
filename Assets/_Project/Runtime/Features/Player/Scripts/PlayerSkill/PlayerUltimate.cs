using UnityEngine;

public abstract class PlayerUltimate : ScriptableObject
{
    [SerializeField] private GameObject dicePrefab;
    [SerializeField] private float gaugeMax = 100f;
    [SerializeField] private float lockDuration = 0.4f;

    public GameObject DicePrefab => dicePrefab;
    public float GaugeMax => gaugeMax;
    public float LockDuration => lockDuration;

    public virtual float GetLockDuration(Player player, int directionSign, Vector3 mouseWorld) => lockDuration;

    public abstract void Execute(Player player, int directionSign, Vector3 mouseWorld);
}