using System.Collections;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Move,
    Chase,
    Attack
}

[RequireComponent(typeof(Rigidbody))]
public class EnemyScript : MonoBehaviour
{
    public EnemyData data;
    public EnemyVision vision;

    [Header("AI")]
    public bool enableNormalAI = true;

    [Header("Combat")]
    public EnemyCombat combat;
    public float attackRangeX = 1.2f;

    [Header("Animation")]
    public string attackStateName = "Attack";

    [Header("Model Facing")]
    public float baseYawForLeft = 0f;

    [Header("Debug")]
    public bool debugLog = false;

    Rigidbody rb;
    Coroutine loop;

    int currentHP;

    public int FacingDir { get; private set; } = -1;
    EnemyState state;

    bool hasAggro = false;
    Transform lockedTarget;

    bool isActionLocked = false;
    Coroutine lockRoutine;

    Animator anim;
    static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");
    static readonly int AnimIsChasing = Animator.StringToHash("IsChasing");
    static readonly int AnimIsAttacking = Animator.StringToHash("IsAttacking");

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        anim = GetComponentInChildren<Animator>();

        if (vision == null) vision = GetComponent<EnemyVision>();
        if (vision != null && vision.enemy == null) vision.enemy = this;

        if (combat == null) combat = GetComponent<EnemyCombat>();

        FacingDir = -1;
        ApplyFacingRotation();
    }

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[Enemy] EnemyData 없음: {name}");
            enabled = false;
            return;
        }

        currentHP = data.maxHP;

        if (enableNormalAI)
            loop = StartCoroutine(AI_Loop());
    }

    IEnumerator AI_Loop()
    {
        while (true)
        {
            if (isActionLocked)
            {
                yield return null;
                continue;
            }

            if (!hasAggro && vision != null && vision.IsDetected && vision.target != null)
            {
                hasAggro = true;
                lockedTarget = vision.target;
                if (debugLog) Debug.Log($"[Enemy] Aggro ON ({name}) target:{lockedTarget.name}");
            }

            if (hasAggro && lockedTarget != null)
            {
                yield return ChaseOrAttackLoop();
                continue;
            }

            yield return PatrolOnce();
        }
    }

    IEnumerator PatrolOnce()
    {
        SetState(EnemyState.Move);

        int dir = Random.value < 0.5f ? -1 : 1;
        SetFacing(dir);

        float startX = rb.position.x;
        float targetX = startX + dir * data.moveDistance;

        while (Mathf.Abs(rb.position.x - targetX) > 0.01f)
        {
            if (isActionLocked) { yield return null; continue; }

            if (!hasAggro && vision != null && vision.IsDetected && vision.target != null)
            {
                hasAggro = true;
                lockedTarget = vision.target;
                yield break;
            }

            MoveX(targetX, data.moveSpeed);
            yield return new WaitForFixedUpdate();
        }

        SetState(EnemyState.Idle);

        float t = data.restTime;
        while (t > 0f)
        {
            if (isActionLocked) { yield return null; continue; }

            if (!hasAggro && vision != null && vision.IsDetected && vision.target != null)
            {
                hasAggro = true;
                lockedTarget = vision.target;
                yield break;
            }

            t -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ChaseOrAttackLoop()
    {
        while (hasAggro && lockedTarget != null)
        {
            if (isActionLocked)
            {
                yield return null;
                continue;
            }

            float dxAbs = Mathf.Abs(lockedTarget.position.x - rb.position.x);

            if (dxAbs <= attackRangeX)
            {
                SetState(EnemyState.Attack);

                if (combat != null && !combat.IsActionActive)
                    combat.BeginAttack();

                yield return null;
                continue;
            }

            SetState(EnemyState.Chase);

            float targetX = lockedTarget.position.x;
            float dx = targetX - rb.position.x;
            SetFacing(dx >= 0f ? 1 : -1);

            MoveX(targetX, data.chaseSpeed);
            yield return new WaitForFixedUpdate();
        }

        SetState(EnemyState.Idle);
    }

    void MoveX(float targetX, float speed)
    {
        Vector3 p = rb.position;
        float nextX = Mathf.MoveTowards(p.x, targetX, speed * Time.fixedDeltaTime);
        rb.MovePosition(new Vector3(nextX, p.y, p.z));
    }

    void SetFacing(int dir)
    {
        int nd = dir >= 0 ? 1 : -1;
        if (FacingDir == nd) return;
        FacingDir = nd;
        ApplyFacingRotation();
    }

    void ApplyFacingRotation()
    {
        float yaw = (FacingDir < 0) ? baseYawForLeft : baseYawForLeft + 180f;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    public void PlayAttackAnimation()
    {
        if (anim != null)
            anim.Play(attackStateName, 0, 0f);
    }

    public void BeginAttackLock(float duration)
    {
        if (!gameObject.activeInHierarchy) return;

        if (lockRoutine != null) StopCoroutine(lockRoutine);
        lockRoutine = StartCoroutine(AttackLockRoutine(duration));
    }

    IEnumerator AttackLockRoutine(float duration)
    {
        isActionLocked = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(duration);

        isActionLocked = false;
        lockRoutine = null;
    }

    void SetState(EnemyState newState)
    {
        if (state == newState) return;
        state = newState;

        if (anim != null)
        {
            bool moving = (state == EnemyState.Move);
            bool chasing = (state == EnemyState.Chase);
            bool attacking = (state == EnemyState.Attack);

            anim.SetBool(AnimIsMoving, moving);
            anim.SetBool(AnimIsChasing, chasing);
            anim.SetBool(AnimIsAttacking, attacking);
        }

        if (debugLog) Debug.Log($"[Enemy] {name} State -> {state}");
    }

    public void GiveUp()
    {
        hasAggro = false;
        lockedTarget = null;
    }

    public int GetCurrentHP() => currentHP;
    public EnemyState GetState() => state;

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0) Destroy(gameObject);
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
        if (lockRoutine != null) StopCoroutine(lockRoutine);
    }
}
