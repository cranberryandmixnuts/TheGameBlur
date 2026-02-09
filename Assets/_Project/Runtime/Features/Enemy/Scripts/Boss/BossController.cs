using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyScript))]
public sealed class BossController : MonoBehaviour
{
    public BossEnemyDataSO bossData;

    public Transform player;
    public Transform projectileOrigin;

    [Header("Underground Fire Spawners (1,2,3)")]
    public Transform undergroundSpawner1;
    public Transform undergroundSpawner2;
    public Transform undergroundSpawner3;

    [Header("Underground Fire Facing (Yaw)")]
    public float lookBackYaw = 180f;
    public float lookScreenYaw = 0f;

    [Header("Anim State Names")]
    public string idleState = "Idle";
    public string moveState = "Move";
    public string jumpState = "Jump";
    public string runState = "Run";
    public string castState = "Cast";
    public string diveState = "Dive";
    public string emergeState = "Emerge";

    [Header("Ground Check")]
    public bool waitGroundBeforeNextSkill = true;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.25f;

    [Header("Dash")]
    public float dashStopOffsetX = 0.8f;

    [Header("Anim Smooth")]
    public float crossFade = 0.08f;

    public bool debugLog = false;

    Rigidbody rb;
    EnemyScript enemy;
    Animator anim;

    int facingDir = -1;
    Coroutine loop;

    int idleHash, moveHash, jumpHash, runHash, castHash, diveHash, emergeHash;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemy = GetComponent<EnemyScript>();
        anim = GetComponentInChildren<Animator>();

        rb.useGravity = true;
        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        if (projectileOrigin == null) projectileOrigin = transform;

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        idleHash = Animator.StringToHash(idleState);
        moveHash = Animator.StringToHash(moveState);
        jumpHash = Animator.StringToHash(jumpState);
        runHash = Animator.StringToHash(runState);
        castHash = Animator.StringToHash(castState);
        diveHash = Animator.StringToHash(diveState);
        emergeHash = Animator.StringToHash(emergeState);

        facingDir = -1;
        ApplyFacingRotation();
    }

    void Start()
    {
        if (bossData == null)
        {
            Debug.LogError($"[Boss] bossData ¾øÀ½: {name}");
            enabled = false;
            return;
        }

        StartCoroutine(StopEnemyAISafely());

        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(SkillLoop());
    }

    IEnumerator StopEnemyAISafely()
    {
        yield return null;
        if (enemy != null)
            enemy.StopAllCoroutines();
    }

    IEnumerator SkillLoop()
    {
        while (true)
        {
            PlayStateIfDifferent(idleHash, false);

            float t = Mathf.Max(0f, bossData.idleBetweenSkills);
            while (t > 0f)
            {
                t -= Time.deltaTime;
                yield return null;
            }

            var skill = PickSkill();
            if (skill == null)
            {
                yield return null;
                continue;
            }

            yield return RunSkill(skill);

            if (waitGroundBeforeNextSkill)
                yield return WaitUntilGrounded();
        }
    }

    IEnumerator WaitUntilGrounded()
    {
        float timeout = 2.5f;
        while (timeout > 0f)
        {
            if (IsGrounded()) yield break;
            timeout -= Time.deltaTime;
            yield return null;
        }
    }

    bool IsGrounded()
    {
        Vector3 origin = rb.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    BossSkillEntry PickSkill()
    {
        if (bossData.skills == null || bossData.skills.Length == 0) return null;

        float total = 0f;
        for (int i = 0; i < bossData.skills.Length; i++)
        {
            var s = bossData.skills[i];
            if (s == null || s.weight <= 0f) continue;
            total += s.weight;
        }
        if (total <= 0f) return null;

        float r = Random.value * total;
        for (int i = 0; i < bossData.skills.Length; i++)
        {
            var s = bossData.skills[i];
            if (s == null || s.weight <= 0f) continue;

            r -= s.weight;
            if (r <= 0f) return s;
        }

        return bossData.skills[bossData.skills.Length - 1];
    }

    IEnumerator RunSkill(BossSkillEntry s)
    {
        if (debugLog) Debug.Log($"[Boss] Skill: {s.type}");

        switch (s.type)
        {
            case BossSkillType.RandomMoveWait:
                yield return Skill_RandomMoveWait(s);
                break;
            case BossSkillType.RandomJumpMove:
                yield return Skill_RandomJumpMove(s);
                break;
            case BossSkillType.DashToPlayerDir:
                yield return Skill_DashToPlayerX(s);
                break;
            case BossSkillType.UndergroundDoubleFire:
                yield return Skill_UndergroundDoubleFire(s);
                break;
            case BossSkillType.TwoShotsToPlayerPos:
                yield return Skill_TwoShotsToPlayerPos(s);
                break;
        }
    }

    IEnumerator Skill_RandomMoveWait(BossSkillEntry s)
    {
        int dir = Random.value < 0.5f ? -1 : 1;
        SetFacing(dir);

        PlayStateForce(moveHash);

        float dist = Random.Range(s.randomMoveDistanceMin, s.randomMoveDistanceMax);
        float targetX = rb.position.x + dir * dist;

        while (Mathf.Abs(rb.position.x - targetX) > 0.01f)
        {
            MoveX(targetX, s.randomMoveSpeed);
            yield return new WaitForFixedUpdate();
        }

        PlayStateIfDifferent(idleHash, false);

        float t = Mathf.Max(0f, s.randomMoveAfterIdle);
        while (t > 0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Skill_RandomJumpMove(BossSkillEntry s)
{
    int dir = Random.value < 0.5f ? -1 : 1;
    SetFacing(dir);

    PlayStateForce(jumpHash);

    float dist = Random.Range(s.jumpDistanceMin, s.jumpDistanceMax);
    float targetX = rb.position.x + dir * dist;

    float duration = Mathf.Max(0.05f, s.jumpDuration);

    float height = Mathf.Max(0f, s.jumpApexHeightMin + Mathf.Abs(dist) * s.jumpApexHeightPerUnit);

    Vector3 from = rb.position;
    Vector3 to = new Vector3(targetX, from.y, from.z);

    yield return ParabolicMove(from, to, height, duration);

    PlayStateIfDifferent(idleHash, false);
}


    IEnumerator ParabolicMove(Vector3 from, Vector3 to, float apexHeight, float duration)
    {
        bool prevKinematic = rb.isKinematic;
        bool prevGravity = rb.useGravity;

        rb.isKinematic = true;
        rb.useGravity = false;

        float t = 0f;

        while (t < duration)
        {
            float u = t / duration;

            float x = Mathf.Lerp(from.x, to.x, u);
            float baseY = Mathf.Lerp(from.y, to.y, u);
            float y = baseY + 4f * apexHeight * u * (1f - u);

            rb.MovePosition(new Vector3(x, y, from.z));

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(new Vector3(to.x, to.y, from.z));

        rb.isKinematic = prevKinematic;
        rb.useGravity = prevGravity;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }


    IEnumerator Skill_DashToPlayerX(BossSkillEntry s)
    {
        if (player == null) yield break;

        float dashDir = (player.position.x - rb.position.x) >= 0f ? 1f : -1f;
        SetFacing(dashDir >= 0f ? 1 : -1);

        float targetX = player.position.x - dashDir * dashStopOffsetX;

        PlayStateForce(runHash);

        float safe = 3f;
        while (safe > 0f && Mathf.Abs(rb.position.x - targetX) > 0.02f)
        {
            MoveX(targetX, s.dashSpeed);
            safe -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator Skill_UndergroundDoubleFire(BossSkillEntry s)
    {
        int savedFacing = facingDir;

        float originX = rb.position.x;
        float outX = originX + Mathf.Abs(s.undergroundWalkDeltaZ);

        PlayStateForce(moveHash);
        SetYaw(lookBackYaw);

        while (Mathf.Abs(rb.position.x - outX) > 0.01f)
        {
            MoveX(outX, s.undergroundWalkSpeed);
            yield return new WaitForFixedUpdate();
        }

        PlayStateIfDifferent(idleHash, false);

        float u = Mathf.Max(0f, s.undergroundDelay);
        while (u > 0f)
        {
            u -= Time.deltaTime;
            yield return null;
        }

        Transform spA;
        Transform spB;

        bool patternA = Random.value < 0.5f;
        if (patternA)
        {
            spA = undergroundSpawner1;
            spB = undergroundSpawner3;
        }
        else
        {
            spA = undergroundSpawner2;
            spB = undergroundSpawner3;
        }

        Vector3 vel = new Vector3(-Mathf.Abs(s.fireHorizontalSpeed), 0f, 0f);

        if (spA != null) SpawnFireball(spA.position, vel);
        if (spB != null) SpawnFireball(spB.position, vel);

        float after = Mathf.Max(0f, s.undergroundAfterFireWait);
        while (after > 0f)
        {
            after -= Time.deltaTime;
            yield return null;
        }

        PlayStateForce(moveHash);
        SetYaw(lookScreenYaw);

        while (Mathf.Abs(rb.position.x - originX) > 0.01f)
        {
            MoveX(originX, s.undergroundWalkSpeed);
            yield return new WaitForFixedUpdate();
        }

        facingDir = savedFacing;
        ApplyFacingRotation();

        PlayStateIfDifferent(idleHash, false);
    }

    IEnumerator Skill_TwoShotsToPlayerPos(BossSkillEntry s)
    {
        PlayStateForce(castHash);

        Vector3 target = player != null ? player.position : (transform.position + Vector3.right * facingDir);

        int count = Mathf.Max(1, s.aimShotCount);
        for (int i = 0; i < count; i++)
        {
            Vector3 p = projectileOrigin.position;
            Vector3 spawn = p + new Vector3(facingDir * s.aimShotSpawnXOffset, s.aimShotSpawnYOffset, 0f);

            Vector3 dir = target - spawn;
            dir.z = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector3.right * facingDir;
            dir.Normalize();

            SetFacing(dir.x >= 0f ? 1 : -1);

            SpawnFireball(spawn, dir * s.aimShotSpeed);

            if (i < count - 1)
                yield return new WaitForSeconds(Mathf.Max(0f, s.aimShotInterval));
        }
    }

    void MoveX(float targetX, float speed)
    {
        Vector3 p = rb.position;
        float nx = Mathf.MoveTowards(p.x, targetX, Mathf.Max(0f, speed) * Time.fixedDeltaTime);
        rb.MovePosition(new Vector3(nx, p.y, p.z));
    }

    void SetYaw(float yaw)
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void SetFacing(int dir)
    {
        int nd = dir >= 0 ? 1 : -1;
        if (facingDir == nd) return;
        facingDir = nd;
        ApplyFacingRotation();
    }

    void ApplyFacingRotation()
    {
        float yaw = (facingDir < 0) ? bossData.baseYawForLeft : bossData.baseYawForLeft + 180f;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void PlayStateIfDifferent(int stateHash, bool restart)
    {
        if (anim == null) return;

        int cur = anim.GetCurrentAnimatorStateInfo(0).shortNameHash;
        if (!restart && cur == stateHash) return;

        anim.CrossFadeInFixedTime(stateHash, crossFade, 0, 0f);
    }

    void PlayStateForce(int stateHash)
    {
        PlayStateIfDifferent(stateHash, true);
    }

    void SpawnFireball(Vector3 pos, Vector3 velocity)
    {
        if (bossData.fireballPrefab == null) return;

        GameObject go = Instantiate(bossData.fireballPrefab, pos, Quaternion.identity);

        var proj = go.GetComponent<BossFireballProjectile>();
        if (proj != null)
        {
            int dmg = (enemy != null && enemy.data != null) ? enemy.data.damage : 1;
            proj.Init(dmg, gameObject, velocity);
            return;
        }

        var r = go.GetComponent<Rigidbody>();
        if (r != null)
        {
            r.useGravity = false;
            r.linearVelocity = velocity;
        }
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
    }
}
