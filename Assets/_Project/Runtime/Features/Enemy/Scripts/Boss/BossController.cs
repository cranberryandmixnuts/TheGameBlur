using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyScript))]
public sealed class BossController : MonoBehaviour
{
    public BossEnemyDataSO bossData;

    public Transform player;
    public Transform projectileOrigin;

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

        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(SkillLoop());
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

        if (IsGrounded())
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
            rb.AddForce(Vector3.up * s.jumpVelocityY, ForceMode.VelocityChange);
        }

        float t = Mathf.Max(0.05f, s.jumpMaxAirTime);
        while (t > 0f && Mathf.Abs(rb.position.x - targetX) > 0.01f)
        {
            MoveX(targetX, s.jumpMoveSpeed);
            t -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
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
        int dir = DirToPlayerOrFacing();
        SetFacing(dir);

        PlayStateForce(diveHash);

        Vector3 originPos = transform.position;

        float td = Mathf.Max(0.01f, s.diveDownTime);
        float downY = originPos.y + s.diveDownOffsetY;
        float t = 0f;
        while (t < td)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / td);
            transform.position = new Vector3(originPos.x, Mathf.Lerp(originPos.y, downY, a), originPos.z);
            yield return null;
        }

        float u = Mathf.Max(0f, s.undergroundDelay);
        while (u > 0f)
        {
            u -= Time.deltaTime;
            yield return null;
        }

        bool topTwo = Random.value < 0.5f;

        Vector3 p = projectileOrigin.position;
        float sx = p.x + dir * s.fireSpawnXOffset;
        Vector3 vel = new Vector3(dir * s.fireHorizontalSpeed, 0f, 0f);

        if (topTwo)
        {
            SpawnFireball(new Vector3(sx, p.y + s.fireTopYOffset, p.z), vel);
            SpawnFireball(new Vector3(sx, p.y + s.fireMidYOffset, p.z), vel);
        }
        else
        {
            SpawnFireball(new Vector3(sx, p.y + s.fireTopYOffset, p.z), vel);
            SpawnFireball(new Vector3(sx, p.y + s.fireBottomYOffset, p.z), vel);
        }

        PlayStateForce(emergeHash);

        float te = Mathf.Max(0.01f, s.emergeTime);
        t = 0f;
        while (t < te)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / te);
            transform.position = new Vector3(originPos.x, Mathf.Lerp(downY, originPos.y, a), originPos.z);
            yield return null;
        }

        transform.position = originPos;
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

    int DirToPlayerOrFacing()
    {
        if (player == null) return facingDir == 0 ? -1 : facingDir;
        float dx = player.position.x - rb.position.x;
        if (Mathf.Abs(dx) < 0.0001f) return facingDir == 0 ? -1 : facingDir;
        return dx >= 0f ? 1 : -1;
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

        anim.CrossFadeInFixedTime(stateHash, crossFade, 0, restart ? 0f : anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
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
