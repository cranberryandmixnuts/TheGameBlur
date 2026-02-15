using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Skill/None", fileName = "Skill_None")]
public sealed class PlayerSkill_None : PlayerSkill
{
    public override bool IsEquipped => false;
    public override bool IsAlwaysUnlocked => true;

    public override Sprite Icon => null;
    public override int ManaCost => 0;
    public override float LockDuration => 0f;
    public override float Cooldown => 0f;

    public override void Execute(Player player, int directionSign, Vector3 mouseWorld) { }
}