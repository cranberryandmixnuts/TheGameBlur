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
public class EnemyScript : MonoBehaviour, IDamageable
{
    public EnemyData data;
    public EnemyVision vision;

    [Header("AI")]
    public bool enableNormalAI = true;

    public bool startAIOnStart = true;

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

    Coroutine aiRoutine; 
    Coroutine moveRoutine;   
    Coroutine lockRoutine;   

    int currentHP;

    public int FacingDir { get; private set; } = -1;
    EnemyState state;

    bool hasAggro = false;
    Transform lockedTarget;

    bool isActionLocked = false;

    bool activeAI = false;   

    Vector3 spawnPos;
    Quaternion spawnRot;

    public bool IsActiveAI => activeAI;

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

        spawnPos = transform.position;
        spawnRot = transform.rotation;

        currentHP = data.maxHP;

        //if (enableNormalAI && startAIOnStart)
        //{
        //    ActivateEnemy(resetHp: false, resetTransform: false);
        //}
        //else
        //{
        //    DeactivateEnemy(resetTransform: false);
        //}
    }


    IEnumerator AI_Loop()
    {
        while (activeAI)
        {
            if (moveRoutine != null)
            {
                yield return null;
                continue;
            }

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


        SetState(EnemyState.Idle);
    }


    public void ActivateEnemy(bool resetHp = false, bool resetTransform = false)
    {
        if (data == null) return;

        activeAI = true;
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        if (resetTransform)
        {
            transform.position = spawnPos;
            transform.rotation = spawnRot;
        }

        if (resetHp) currentHP = data.maxHP;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        hasAggro = false;
        lockedTarget = null;
        isActionLocked = false;

        if (lockRoutine != null)
        {
            StopCoroutine(lockRoutine);
            lockRoutine = null;
        }

        activeAI = true;

        if (combat != null) combat.enabled = true;

        if (enableNormalAI)
        {
            if (aiRoutine != null) StopCoroutine(aiRoutine);
            aiRoutine = StartCoroutine(AI_Loop());
        }

        SetState(EnemyState.Idle);

        if (debugLog) Debug.Log($"[Enemy] Activate ({name})");
    }

    public void DeactivateEnemy(bool resetTransform = false)
    {
        activeAI = false;

        if (aiRoutine != null)
        {
            StopCoroutine(aiRoutine);
            aiRoutine = null;
        }

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        if (lockRoutine != null)
        {
            StopCoroutine(lockRoutine);
            lockRoutine = null;
        }

        if (combat != null) combat.enabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        hasAggro = false;
        lockedTarget = null;
        isActionLocked = false;

        SetState(EnemyState.Idle);

        if (resetTransform)
        {
            transform.position = spawnPos;
            transform.rotation = spawnRot;
        }

        if (debugLog) Debug.Log($"[Enemy] Deactivate ({name})");
    }

    public void SetCombatEnabled(bool enabled)
    {
        if (combat != null)
            combat.enabled = enabled;
    }

    public void MoveToTarget(Transform target, float speed, bool resumeAIWhenArrived = true)
    {
        if (target == null) return;


        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        activeAI = false;
        if (aiRoutine != null)
        {
            StopCoroutine(aiRoutine);
            aiRoutine = null;
        }

        hasAggro = false;
        lockedTarget = null;
        isActionLocked = false;

        if (combat != null) combat.enabled = false;

        moveRoutine = StartCoroutine(MoveToTargetRoutine(target, speed, resumeAIWhenArrived));
    }

    IEnumerator MoveToTargetRoutine(Transform target, float speed, bool resumeAIWhenArrived)
    {
        SetState(EnemyState.Move);

        while (target != null)
        {
            float dx = target.position.x - rb.position.x;

            if (Mathf.Abs(dx) <= 0.05f)
                break;

            SetFacing(dx >= 0f ? 1 : -1);

            MoveX(target.position.x, speed);

            yield return new WaitForFixedUpdate();
        }

        SetState(EnemyState.Idle);
        moveRoutine = null;

        
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
            if (!activeAI) yield break;
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
            if (!activeAI) yield break;
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
        while (activeAI && hasAggro && lockedTarget != null)
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

                if (combat != null && combat.enabled && !combat.IsActionActive)
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
        float nextX = Mathf.MoveTowards(p.x, targetX, Mathf.Max(0f, speed) * Time.fixedDeltaTime);
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

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

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

    public void ResetHP()
    {
        currentHP = data.maxHP;
    }

    public int GetCurrentHP() => currentHP;
    public EnemyState GetState() => state;


    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0) Destroy(gameObject);
    }

    public void ApplyDamage(DamagePayload payload)
    {
        var rng = GetComponent<EnemyCombatRng>();
        if (rng != null && rng.TryEvadeIncomingDamage())
            return;

        TakeDamage(payload.Amount);
    }

    void OnDisable()
    {
        if (aiRoutine != null) { StopCoroutine(aiRoutine); aiRoutine = null; }
        if (moveRoutine != null) { StopCoroutine(moveRoutine); moveRoutine = null; }
        if (lockRoutine != null) { StopCoroutine(lockRoutine); lockRoutine = null; }
    }
}
