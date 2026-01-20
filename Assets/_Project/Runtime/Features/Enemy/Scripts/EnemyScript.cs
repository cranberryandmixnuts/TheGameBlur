// EnemyScript.cs
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

    [Header("Combat")]
    public EnemyCombat combat;
    public float attackRangeX = 1.2f;

    Rigidbody rb;
    Coroutine loop;

    int currentHP;
    int damage;

    public int FacingDir { get; private set; } = -1;

    EnemyState state;

    bool hasAggro = false;
    Transform lockedTarget;

    bool isActionLocked = false;
    Coroutine lockRoutine;

    Animator anim;
    static readonly int AnimIsMoving = Animator.StringToHash("IsMoving");

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
        UpdateFacingRotation();
    }

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[Enemy] EnemyData ¾øÀ½: {name}");
            enabled = false;
            return;
        }

        currentHP = data.maxHP;
        damage = data.damage;

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
            }

            if (hasAggro && lockedTarget != null)
            {
                yield return ChaseLoop();
                continue;
            }

            yield return PatrolOnce();
        }
    }

    IEnumerator PatrolOnce()
    {
        SetState(EnemyState.Move);

        int dir = Random.value < 0.5f ? -1 : 1;
        FacingDir = dir;
        UpdateFacingRotation();

        Vector3 start = rb.position;
        Vector3 target = start + Vector3.right * dir * data.moveDistance;

        while ((rb.position - target).sqrMagnitude > 0.001f)
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
                yield break;
            }

            Vector3 next = Vector3.MoveTowards(rb.position, target, data.moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(target);
        SetState(EnemyState.Idle);

        float t = data.restTime;
        while (t > 0f)
        {
            if (isActionLocked)
            {
                yield return null;
                continue;
            }

            t -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ChaseLoop()
    {
        SetState(EnemyState.Chase);

        while (hasAggro && lockedTarget != null)
        {
            if (isActionLocked)
            {
                yield return null;
                continue;
            }

            if (combat != null && !combat.IsActionActive)
            {
                float dxAbs = Mathf.Abs(lockedTarget.position.x - rb.position.x);
                if (dxAbs <= attackRangeX)
                {
                    combat.BeginAttack(); 
                    yield return null;   
                    continue;
                }
            }

            float targetX = lockedTarget.position.x;
            float myY = rb.position.y;
            float myZ = rb.position.z;

            float dx = targetX - rb.position.x;
            FacingDir = (dx >= 0f) ? 1 : -1;
            UpdateFacingRotation();

            Vector3 chasePos = new Vector3(targetX, myY, myZ);
            Vector3 next = Vector3.MoveTowards(rb.position, chasePos, data.moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(next);

            yield return new WaitForFixedUpdate();
        }

        SetState(EnemyState.Idle);
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
        SetState(EnemyState.Attack);

        yield return new WaitForSeconds(duration);

        isActionLocked = false;
        lockRoutine = null;

        SetState(EnemyState.Idle);
    }


    public void GiveUp()
    {
        hasAggro = false;
        lockedTarget = null;
    }

    void UpdateFacingRotation()
    {

        if (FacingDir < 0)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        else
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    void SetState(EnemyState newState)
    {
        if (state == newState) return;
        state = newState;

        if (anim != null)
        {
            bool moving = (state == EnemyState.Move || state == EnemyState.Chase);
            anim.SetBool(AnimIsMoving, moving);
        }
    }

    public int GetCurrentHP() => currentHP;
    public int GetDamage() => damage;
    public EnemyGrade GetGrade() => data.grade;
    public EnemyState GetState() => state;

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        if (loop != null) StopCoroutine(loop);
        if (lockRoutine != null) StopCoroutine(lockRoutine);
        Destroy(gameObject);
    }
}
