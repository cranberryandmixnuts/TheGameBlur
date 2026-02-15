using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player/Skills/Fireball", fileName = "Skill_Fireball")]
public sealed class FireballPlayerSkill : PlayerSkill
{
    [Header("Projectile")]
    [SerializeField] private FireballProjectile projectilePrefab;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxDistance = 12f;
    [SerializeField] private float spawnOffset = 1.0f;

    [Header("Damage")]
    [SerializeField] private int damage = 12;
    [SerializeField] private float hitRadius = 0.35f;

    public override void Execute(Player player, int directionSign, Vector3 mouseWorld)
    {
        int diceValue = player.Stats.DiceValue;
        float skillSize = DiceChanceTable.GetPlayerSkillSize(diceValue);

        int finalDamage = damage;
        float critChance = DiceChanceTable.GetPlayerChance(diceValue);
        if (critChance > 0f && Random.value < critChance) finalDamage *= 2;

        Vector3 p = player.transform.position;

        Vector3 d = new Vector3(mouseWorld.x - p.x, mouseWorld.y - p.y, 0f);
        if (d.sqrMagnitude < 0.0001f) d = Vector3.right * directionSign;

        d.Normalize();

        Vector3 spawn = p + d * spawnOffset;
        spawn.z = player.Settings.planeZ;

        FireballProjectile proj = Instantiate(projectilePrefab, spawn, Quaternion.identity);
        proj.Initialize(d, speed, maxDistance, finalDamage, hitRadius * skillSize, player.Settings.attackMask, player.Settings.groundMask, player.gameObject, player.Settings.planeZ, skillSize);
    }
}