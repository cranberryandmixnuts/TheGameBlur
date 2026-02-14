using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player/Ultimates/Debug Ultimate", fileName = "Ultimate_Debug")]
public sealed class DebugPlayerUltimate : PlayerUltimate
{
    [SerializeField] private string message = "Debug Ultimate executed";

    public override void Execute(Player player, int directionSign, Vector3 mouseWorld) => Debug.Log(message);
}