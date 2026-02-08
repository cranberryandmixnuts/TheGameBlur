using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("연결")]
    public EnemyScript enemy;
    public Transform target;

    [Header("시야 설정")]
    public float viewRadius = 6f;
    [Range(0f, 180f)] public float viewAngle = 60f;

    [Header("레이어")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public bool IsDetected { get; private set; }

    void Reset()
    {
        enemy = GetComponent<EnemyScript>();
    }

    void Update()
    {
        Detect();
        DrawDebug();
    }

    void Detect()
    {
        IsDetected = false;
        if (enemy == null) return;

        Vector3 origin = transform.position;

        Transform t = target;
        if (t == null)
        {
            Collider[] hits = Physics.OverlapSphere(origin, viewRadius, targetMask);
            if (hits.Length > 0)
                t = hits[0].transform;
        }

        if (t == null) return;

        Vector3 toTarget = t.position - origin;
        toTarget.z = 0f;

        Vector3 forwardDir = (enemy.FacingDir >= 0) ? Vector3.right : Vector3.left;

        float angle = Vector3.Angle(forwardDir, toTarget);
        if (angle > viewAngle * 0.5f) return;

        float dist = toTarget.magnitude;
        if (Physics.Raycast(origin, toTarget.normalized, dist, obstacleMask))
            return;

        target = t;
        IsDetected = true;
    }

    void DrawDebug()
    {
        if (enemy == null) return;

        Vector3 forwardDir = (enemy.FacingDir >= 0) ? Vector3.right : Vector3.left;

        Debug.DrawRay(transform.position, forwardDir * viewRadius, Color.red);

        Vector3 left = DirFromAngle(-viewAngle * 0.5f, forwardDir);
        Vector3 right = DirFromAngle(viewAngle * 0.5f, forwardDir);

        Debug.DrawRay(transform.position, left * viewRadius, Color.yellow);
        Debug.DrawRay(transform.position, right * viewRadius, Color.yellow);
    }

    Vector3 DirFromAngle(float angleDeg, Vector3 forward)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float x = forward.x * Mathf.Cos(rad) - forward.y * Mathf.Sin(rad);
        float y = forward.x * Mathf.Sin(rad) + forward.y * Mathf.Cos(rad);
        return new Vector3(x, y, 0f).normalized;
    }
}
