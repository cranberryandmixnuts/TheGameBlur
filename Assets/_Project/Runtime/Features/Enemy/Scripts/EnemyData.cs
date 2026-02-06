using UnityEngine;

public enum EnemyGrade
{
    A, B, C, D, F
}

[CreateAssetMenu(fileName = "EnemyData_", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("등급")]
    public EnemyGrade grade = EnemyGrade.F;

    [Header("기본 스탯")]
    public int maxHP = 10;
    public int damage = 1;

    [Header("이동")]
    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public float restTime = 1f;

    [Header("추격")]
    public float chaseSpeed = 3.5f;



    [Header("공격 타이밍")]
    public float attackAnimTime = 0.6f;

    [Range(0f, 1f)]
    public float hitNormalizedTime = 0.35f;

    [Header("공격 히트박스")]
    public Vector3 hitboxHalfExtents = new Vector3(0.6f, 0.5f, 0.6f);

    [Tooltip("적 기준 앞쪽으로 히트박스를 얼마나 이동시킬지")]
    public float hitboxForwardOffset = 0.9f;

    [Header("공격 대상 레이어")]
    public LayerMask playerMask;
}
