using System;
using UnityEngine;

public enum BossSkillType
{
    RandomMoveWait,
    RandomJumpMove,
    DashToPlayerDir,
    UndergroundDoubleFire,
    TwoShotsToPlayerPos
}

[Serializable]
public class BossSkillEntry
{
    public BossSkillType type;
    [Min(0f)] public float weight = 1f;

    public float randomMoveSpeed = 3f;
    public float randomMoveDistanceMin = 2f;
    public float randomMoveDistanceMax = 5f;
    public float randomMoveAfterIdle = 1.5f;

    public float jumpMoveSpeed = 4.5f;
    public float jumpDistanceMin = 2f;
    public float jumpDistanceMax = 5f;
    public float jumpDuration = 1f;
    public float jumpApexHeightMin = 1.5f;
    public float jumpApexHeightPerUnit = 0.25f;


    public float dashSpeed = 10f;
    public float dashDistance = 7f;

    public float diveDownOffsetY = -3f;
    public float diveDownTime = 0.12f;
    public float undergroundDelay = 0.15f;
    public float emergeTime = 0.12f;

    public float undergroundWalkDeltaZ = 2f;
    public float undergroundWalkSpeed = 3f;
    public float undergroundAfterFireWait = 1.2f;

    public float fireHorizontalSpeed = 12f;
    public float fireSpawnXOffset = 0.8f;
    public float fireTopYOffset = 1.2f;
    public float fireMidYOffset = 0.5f;
    public float fireBottomYOffset = -1.2f;

    public float aimShotSpeed = 12f;
    public float aimShotInterval = 0.5f;
    public int aimShotCount = 2;
    public float aimShotSpawnXOffset = 0.8f;
    public float aimShotSpawnYOffset = 0.8f;
}

[CreateAssetMenu(fileName = "BossData_", menuName = "Game/Boss Data")]
public class BossEnemyDataSO : ScriptableObject
{
    [Min(0f)] public float idleBetweenSkills = 1f;
    public BossSkillEntry[] skills;

    public GameObject fireballPrefab;

    public float baseYawForLeft = 0f;
}
