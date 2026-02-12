using UnityEngine;

public abstract class PlayerSkill : ScriptableObject
{
    [SerializeField] private int manaCost = 20;
    [SerializeField] private float lockDuration = 0.2f;

    public int ManaCost => manaCost;
    public float LockDuration => lockDuration;

    public abstract void Execute(Player player, int directionSign, Vector3 mouseWorld);
}