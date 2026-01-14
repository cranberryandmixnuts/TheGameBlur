using UnityEngine;

public enum EnemyGrade
{
    A, B, C, D, F
}


[CreateAssetMenu(fileName = "EnemyData_", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public EnemyGrade grade = EnemyGrade.F;

    public int maxHP = 10;
    public int damage = 1;

    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public float restTime = 1f;

    public float fixedZ = 0f;
}


