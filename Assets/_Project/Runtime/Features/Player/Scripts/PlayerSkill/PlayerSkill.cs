using UnityEngine;

public abstract class PlayerSkill : ScriptableObject
{
    [SerializeField] private Sprite icon;
    [SerializeField] private int manaCost = 20;
    [SerializeField] private float lockDuration = 0.2f;
    [SerializeField] private float cooldown = 1.0f;

    public Sprite Icon => icon;
    public int ManaCost => manaCost;
    public float LockDuration => lockDuration;
    public float Cooldown => cooldown;

    public abstract void Execute(Player player, int directionSign, Vector3 mouseWorld);
}