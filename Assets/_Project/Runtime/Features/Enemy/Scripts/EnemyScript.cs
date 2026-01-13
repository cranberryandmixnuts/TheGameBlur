using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyScript : MonoBehaviour
{
    public EnemyDataSO data;

    Rigidbody rb;
    Coroutine loop;

    int currentHP;
    int damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        Debug.Log($"[EnemyScript] Awake - Rigidbody 설정 완료 ({name})");
    }

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyScript] EnemyDataSO 없음 ({name})");
            enabled = false;
            return;
        }

        currentHP = data.maxHP;
        damage = data.damage;

        Debug.Log($"[EnemyScript] Start - HP:{currentHP}, DMG:{damage}, Speed:{data.moveSpeed}, Dist:{data.moveDistance}");

        loop = StartCoroutine(MoveRestLoop());
    }

    IEnumerator MoveRestLoop()
    {
        Debug.Log("[EnemyScript] 이동 루프 시작");

        while (true)
        {
            int dir = Random.value < 0.5f ? -1 : 1;

            Vector3 start = rb.position;
            Vector3 target = start + Vector3.right * dir * data.moveDistance;

            Debug.Log($"[EnemyScript] 이동 시작 | 방향:{(dir == 1 ? "Right" : "Left")} | 시작:{start} → 목표:{target}");

            while ((rb.position - target).sqrMagnitude > 0.001f)
            {
                Vector3 next = Vector3.MoveTowards(
                    rb.position,
                    target,
                    data.moveSpeed * Time.fixedDeltaTime
                );

                rb.MovePosition(next);

                Debug.Log($"[EnemyScript] 이동 중 | 현재:{rb.position}");

                yield return new WaitForFixedUpdate();
            }

            rb.MovePosition(target);

            Debug.Log($"[EnemyScript] 이동 완료 | 도착:{target} | {data.restTime}초 대기");

            yield return new WaitForSeconds(data.restTime);
        }
    }

    public int GetCurrentHP() => currentHP;
    public int GetDamage() => damage;
    public EnemyGrade GetGrade() => data.grade;

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        Debug.Log($"[EnemyScript] 데미지 받음 {amount} | 남은 HP:{currentHP}");

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log($"[EnemyScript] 사망 ({name})");

        if (loop != null)
            StopCoroutine(loop);

        Destroy(gameObject);
    }
}
