// BossController.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class BossController : MonoBehaviour
{
    public BossEnemyDataSO bossData;

    public Transform player;

    [Header("Underground Spawners")]
    public Transform undergroundSpawner1;
    public Transform undergroundSpawner2;
    public Transform undergroundSpawner3;

    [Header("Model Root (Rotate This)")]
    public Transform modelRoot;

    [Header("Anim State Names")]
    public string idleState = "Idle";
    public string moveState = "Move";
    public string jumpState = "Jump";
    public string runState = "Run";
    public string castState = "Cast";

    [Header("Underground Facing (Delta From baseYawForLeft)")]
    public float undergroundBackYawOffset = 90f;
    public float undergroundFrontYawOffset = -90f;

    [Header("Aim Shot Spawn Offset (From Boss Body)")]
    public float aimShotSpawnXOffset = 0.8f;
    public float aimShotSpawnYOffset = 0.8f;

    [Header("Dash")]
    public float dashStopOffsetX = 0.8f;

    [Header("Anim Smooth")]
    public float crossFade = 0.08f;

    public bool debugLog = false;

    Rigidbody rb;
    Animator anim;

    int facingDir = -1;
    Coroutine loop;

    int idleHash, moveHash, jumpHash, runHash, castHash;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        rb.useGravity = true;
        rb.constraints =
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (modelRoot == null)
            modelRoot = (anim != null) ? anim.transform : transform;

        idleHash = Animator.StringToHash(idleState);
        moveHash = Animator.StringToHash(moveState);
        jumpHash = Animator.StringToHash(jumpState);
        runHash = Animator.StringToHash(runState);
        castHash = Animator.StringToHash(castState);
    }

    void Start()
    {
        if (bossData == null)
        {
            Debug.LogError($"[Boss] bossData ¾øÀ½: {name}");
            enabled = false;
            return;
        }

        facingDir = -1;
        ApplyFacingRotation();

        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(SkillLoop());
    }

    IEnumerator SkillLoop()
    {
        while (true)
        {
            PlayStateForce(idleHash);
            yield return WaitSeconds(bossData.idleBetweenSkills);

            var skill = PickSkill();
            if (skill == null)
            {
                yield return null;
                continue;
            }

            yield return RunSkill(skill);
        }
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

        PlayStateForce(idleHash);
        yield return WaitSeconds(s.randomMoveAfterIdle);
    }

    IEnumerator Skill_RandomJumpMove(BossSkillEntry s)
    {
        int dir = Random.value < 0.5f ? -1 : 1;
        SetFacing(dir);

        PlayStateForce(jumpHash);

        float dist = Random.Range(s.jumpDistanceMin, s.jumpDistanceMax);
        float startX = rb.position.x;
        float startY = rb.position.y;
        float startZ = rb.position.z;

        float targetX = startX + dir * dist;

        float duration = Mathf.Max(0.01f, s.jumpDuration);
        float apex = Mathf.Max(0f, s.jumpApexHeightMin + Mathf.Abs(dist) * s.jumpApexHeightPerUnit);

        bool prevGravity = rb.useGravity;
        rb.useGravity = false;

        float t = 0f;
        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            float u = Mathf.Clamp01(t / duration);

            float x = Mathf.Lerp(startX, targetX, u);
            float y = startY + 4f * apex * u * (1f - u);

            rb.MovePosition(new Vector3(x, y, startZ));
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(new Vector3(targetX, startY, startZ));
        rb.useGravity = prevGravity;

        PlayStateForce(idleHash);
    }

    IEnumerator Skill_DashToPlayerX(BossSkillEntry s)
    {
        if (player == null) yield break;

        float dashDir = (player.position.x - rb.position.x) >= 0f ? 1f : -1f;
        SetFacing(dashDir >= 0f ? 1 : -1);

        float targetX = player.position.x - dashDir * dashStopOffsetX;

        PlayStateForce(runHash);

        float safe = 2.5f;
        while (safe > 0f && Mathf.Abs(rb.position.x - targetX) > 0.02f)
        {
            MoveX(targetX, s.dashSpeed);
            safe -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        PlayStateForce(idleHash);
    }

    IEnumerator Skill_UndergroundDoubleFire(BossSkillEntry s)
    {
        int prevFacing = facingDir;

        Vector3 startPos = rb.position;
        float startZ = startPos.z;
        float outZ = startZ + s.undergroundWalkDeltaZ;

        var savedConstraints = rb.constraints;
        rb.constraints = savedConstraints & ~RigidbodyConstraints.FreezePositionZ;

        float backYaw = bossData.baseYawForLeft + undergroundBackYawOffset;
        SetModelYaw(backYaw);

        PlayStateForce(moveHash);
        yield return MoveZ(outZ, s.undergroundWalkSpeed);

        bool prevGravity = rb.useGravity;
        rb.useGravity = false;

        float downY = startPos.y - Mathf.Abs(s.undergroundDropY);
        yield return MoveY(downY, Mathf.Max(0.01f, s.undergroundDropTime));

        PlayStateForce(idleHash);
        yield return WaitSeconds(s.undergroundBeforeFireWait);

        PlayStateForce(castHash);

        bool patternA = Random.value < 0.5f;
        Transform a = undergroundSpawner1;
        Transform b = patternA ? undergroundSpawner2 : undergroundSpawner3;

        Vector3 vel = Vector3.left * Mathf.Max(0f, s.fireHorizontalSpeed);

        if (a != null) SpawnFireball(a.position, vel);
        if (b != null) SpawnFireball(b.position, vel);

        PlayStateForce(idleHash);
        yield return WaitSeconds(s.undergroundAfterFireWait);

        yield return MoveY(startPos.y, Mathf.Max(0.01f, s.undergroundRiseTime));

        rb.useGravity = prevGravity;

        float frontYaw = bossData.baseYawForLeft + undergroundFrontYawOffset;
        SetModelYaw(frontYaw);

        PlayStateForce(moveHash);
        yield return MoveZ(startZ, s.undergroundWalkSpeed);

        rb.constraints = savedConstraints;

        SetFacing(prevFacing);
        PlayStateForce(idleHash);
    }

    IEnumerator Skill_TwoShotsToPlayerPos(BossSkillEntry s)
    {
        PlayStateForce(castHash);

        Vector3 lockedTarget = player != null
            ? player.position
            : (rb.position + Vector3.left);

        lockedTarget.z = rb.position.z;

        int count = Mathf.Max(1, s.aimShotCount);
        float interval = Mathf.Max(0f, s.aimShotInterval);
        float speed = Mathf.Max(0f, s.aimShotSpeed);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawn = rb.position + new Vector3(facingDir * aimShotSpawnXOffset, aimShotSpawnYOffset, 0f);

            Vector3 dir = lockedTarget - spawn;
            dir.z = 0f;

            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector3.left;

            dir.Normalize();

            SetFacing(dir.x >= 0f ? 1 : -1);

            spawn = rb.position + new Vector3(facingDir * aimShotSpawnXOffset, aimShotSpawnYOffset, 0f);

            SpawnFireball(spawn, dir * speed);

            if (i < count - 1)
                yield return WaitSeconds(interval);
        }

        PlayStateForce(idleHash);
    }

    IEnumerator MoveZ(float targetZ, float speed)
    {
        while (Mathf.Abs(rb.position.z - targetZ) > 0.01f)
        {
            Vector3 p = rb.position;
            float nz = Mathf.MoveTowards(p.z, targetZ, Mathf.Max(0f, speed) * Time.fixedDeltaTime);
            rb.MovePosition(new Vector3(p.x, p.y, nz));
            yield return new WaitForFixedUpdate();
        }

        Vector3 pe = rb.position;
        rb.MovePosition(new Vector3(pe.x, pe.y, targetZ));
    }

    IEnumerator MoveY(float targetY, float duration)
    {
        float startY = rb.position.y;

        float t = 0f;
        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            float a = Mathf.Clamp01(t / duration);

            Vector3 p = rb.position;
            float y = Mathf.Lerp(startY, targetY, a);
            rb.MovePosition(new Vector3(p.x, y, p.z));

            yield return new WaitForFixedUpdate();
        }

        Vector3 pe = rb.position;
        rb.MovePosition(new Vector3(pe.x, targetY, pe.z));
    }

    void MoveX(float targetX, float speed)
    {
        Vector3 p = rb.position;
        float nx = Mathf.MoveTowards(p.x, targetX, Mathf.Max(0f, speed) * Time.fixedDeltaTime);
        rb.MovePosition(new Vector3(nx, p.y, p.z));
    }

    IEnumerator WaitSeconds(float seconds)
    {
        float t = Mathf.Max(0f, seconds);
        while (t > 0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }
    }

    void SpawnFireball(Vector3 pos, Vector3 velocity)
    {
        if (bossData.fireballPrefab == null) return;

        GameObject go = Instantiate(bossData.fireballPrefab, pos, Quaternion.identity);

        var proj = go.GetComponent<BossFireballProjectile>();
        if (proj != null)
        {
            proj.Init(1, gameObject, velocity);
            return;
        }

        var r = go.GetComponent<Rigidbody>();
        if (r != null)
        {
            r.useGravity = false;
            r.linearVelocity = velocity;
        }
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
        if (bossData == null) return;

        float yaw = (facingDir < 0) ? bossData.baseYawForLeft : bossData.baseYawForLeft + 180f;
        SetModelYaw(yaw);
    }

    void SetModelYaw(float yaw)
    {
        if (modelRoot == null) return;

        if (modelRoot == transform)
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        else
            modelRoot.localRotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void PlayStateForce(int stateHash)
    {
        if (anim == null) return;
        anim.CrossFadeInFixedTime(stateHash, crossFade, 0, 0f);
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
    }
}
