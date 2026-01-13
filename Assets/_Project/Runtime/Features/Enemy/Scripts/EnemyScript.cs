using System.Collections;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Move
}

[RequireComponent(typeof(Rigidbody))]
public class EnemyScript : MonoBehaviour
{
    public EnemyDataSO data;

    Rigidbody rb;
    Coroutine loop;

    int currentHP;
    int damage;

    public int FacingDir { get; private set; } = -1;

    EnemyState state;

    public bool useDebugColor = true;

    Renderer rend;
    Material mat;
    Color originalColor;
    readonly Color idleColor = new Color(1f, 0.5f, 0f);

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

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

        if (useDebugColor)
        {
            rend = GetComponent<Renderer>();
            if (rend != null)
            {
                mat = rend.material;
                originalColor = GetMatColor();
            }
        }

        FacingDir = -1;
    }

    void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[Enemy] data(EnemyDataSO) ¾øÀ½: {name}");
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
            SetState(EnemyState.Move);

            int dir = Random.value < 0.5f ? -1 : 1;

            FacingDir = dir;

            Vector3 start = rb.position;
            Vector3 target = start + Vector3.right * dir * data.moveDistance;

            while ((rb.position - target).sqrMagnitude > 0.001f)
            {
                Vector3 next = Vector3.MoveTowards(rb.position, target, data.moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(next);
                yield return new WaitForFixedUpdate();
            }

            rb.MovePosition(target);

            SetState(EnemyState.Idle);

            if (data.restTime > 0f)
                yield return new WaitForSeconds(data.restTime);
            else
                yield return null;
        }
    }

    void SetState(EnemyState newState)
    {
        if (state == newState) return;
        state = newState;

        if (anim != null)
        {
            bool isMoving = (state == EnemyState.Move);
            anim.SetBool(AnimIsMoving, isMoving);
        }

        if (useDebugColor)
        {
            if (state == EnemyState.Idle) SetMatColor(idleColor);
            else SetMatColor(originalColor);
        }
    }

    Color GetMatColor()
    {
        if (mat == null) return Color.white;

        if (mat.HasProperty(BaseColorId)) return mat.GetColor(BaseColorId);
        if (mat.HasProperty(ColorId)) return mat.GetColor(ColorId);

        return Color.white;
    }

    void SetMatColor(Color c)
    {
        if (mat == null) return;

        if (mat.HasProperty(BaseColorId)) mat.SetColor(BaseColorId, c);
        else if (mat.HasProperty(ColorId)) mat.SetColor(ColorId, c);
    }

    public int GetCurrentHP() => currentHP;
    public int GetDamage() => damage;
    public EnemyGrade GetGrade() => data.grade;
    public EnemyState GetState() => state;

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (loop != null) StopCoroutine(loop);
        Destroy(gameObject);
    }
}
