using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player/Ultimates/SliceAndDice", fileName = "Ultimate_SliceAndDice")]
public sealed class BasicPlayerUltimate : PlayerUltimate
{
    public override float GaugeMax => 70f;
    public override bool UseFixedUltimateDice => true;
    public override int FixedUltimateDiceA => 6;
    public override int FixedUltimateDiceB => 6;

    public override void Execute(Player player, int directionSign, Vector3 mouseWorld) { }
}