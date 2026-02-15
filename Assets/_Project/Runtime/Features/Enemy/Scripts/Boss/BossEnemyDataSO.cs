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

    public float jumpDistanceMin = 2f;
    public float jumpDistanceMax = 5f;
    public float jumpDuration = 1f;
    public float jumpApexHeightMin = 1.5f;
    public float jumpApexHeightPerUnit = 0.25f;

    public float dashSpeed = 10f;

    public float undergroundWalkDeltaZ = 2f;
    public float undergroundWalkOutSpeed = 12f;
    public float undergroundWalkInSpeed = 12f;

    public float undergroundDropY = 2f;
    public float undergroundDropTime = 0.12f;
    public float undergroundBeforeFireWait = 0.0f;
    public float undergroundAfterFireWait = 1.2f;
    public float undergroundRiseTime = 0.12f;

    public float fireHorizontalSpeed = 12f;

    public float aimShotSpeed = 12f;
    public float aimShotInterval = 0.5f;
    public int aimShotCount = 2;
}

[CreateAssetMenu(fileName = "BossData_", menuName = "Game/Boss Data")]
public class BossEnemyDataSO : ScriptableObject
{
    [Min(0f)] public float idleBetweenSkills = 1f;
    public BossSkillEntry[] skills;

    public GameObject fireballPrefab;

    public float baseYawForLeft = 0f;
}
