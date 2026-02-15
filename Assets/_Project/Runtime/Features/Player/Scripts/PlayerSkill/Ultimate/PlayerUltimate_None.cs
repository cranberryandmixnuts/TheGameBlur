using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Ultimate/None", fileName = "Ultimate_None")]
public sealed class PlayerUltimate_None : PlayerUltimate
{
    public override bool IsAlwaysUnlocked => true;

    public override float GaugeMax => 0f;
    public override bool DiceEnabled => false;
    public override bool UseFixedDice => true;
    public override int FixedDiceA => 1;
    public override int FixedDiceB => 6;

    public override float GetLockDuration(Player player, int directionSign, Vector3 mouseWorld) => 0f;

    public override void Execute(Player player, int directionSign, Vector3 mouseWorld) { }
}