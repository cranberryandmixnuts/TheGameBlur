using System;
using UnityEngine;

public abstract class PlayerSkill : ScriptableObject
{
    [SerializeField] private Sprite icon;
    [SerializeField] private int manaCost = 20;
    [SerializeField] private float lockDuration = 0.2f;
    [SerializeField] private float cooldown = 1.0f;

    public virtual bool IsEquipped => true;
    public virtual bool IsAlwaysUnlocked => false;

    public virtual Sprite Icon => icon;
    public virtual int ManaCost => manaCost;
    public virtual float LockDuration => lockDuration;
    public virtual float Cooldown => cooldown;

    public abstract void Execute(Player player, int directionSign, Vector3 mouseWorld);
}