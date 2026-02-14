using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player/Skills/Debug Skill", fileName = "Skill_Debug")]
public sealed class DebugPlayerSkill : PlayerSkill
{
    [SerializeField] private string message = "Debug Skill executed";

    public override void Execute(Player player, int directionSign, Vector3 mouseWorld) => Debug.Log(message);
}