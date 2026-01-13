using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public EnemyScript enemy;

    public float viewRadius = 8f;
    [Range(0f, 180f)] public float viewAngle = 60f;

    public LayerMask targetMask;   
    public LayerMask obstacleMask;

    public Transform detectedTarget; 

    void Reset()
    {
        enemy = GetComponent<EnemyScript>();
    }

    void Update()
    {
        Detect();
        DrawDebugRay();
    }

    void Detect()
    {
        detectedTarget = null;

        Vector3 origin = transform.position;

        Collider[] hits = Physics.OverlapSphere(origin, viewRadius, targetMask);
        if (hits == null || hits.Length == 0) return;

        int facing = (enemy != null) ? enemy.FacingDir : -1;
        Vector3 forwardDir = (facing >= 0) ? Vector3.right : Vector3.left;

        foreach (var col in hits)
        {
            Vector3 toTarget = col.transform.position - origin;
            toTarget.z = 0f;

            float angle = Vector3.Angle(forwardDir, toTarget);
            if (angle > viewAngle * 0.5f) continue;

            float dist = toTarget.magnitude;
            if (Physics.Raycast(origin, toTarget.normalized, dist, obstacleMask))
                continue;

            detectedTarget = col.transform;
            break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        int facing = -1;
        if (enemy != null) facing = enemy.FacingDir;

        Vector3 forwardDir = (facing >= 0) ? Vector3.right : Vector3.left;

        Vector3 leftBound = DirFromAngle(-viewAngle * 0.5f, forwardDir);
        Vector3 rightBound = DirFromAngle(viewAngle * 0.5f, forwardDir);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBound * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBound * viewRadius);

        if (detectedTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, detectedTarget.position);
        }
    }

    Vector3 DirFromAngle(float angleDeg, Vector3 forwardDir)
    {
        float rad = angleDeg * Mathf.Deg2Rad;

        float x = forwardDir.x * Mathf.Cos(rad) - forwardDir.y * Mathf.Sin(rad);
        float y = forwardDir.x * Mathf.Sin(rad) + forwardDir.y * Mathf.Cos(rad);

        return new Vector3(x, y, 0f).normalized;
    }

    void DrawDebugRay()
    {
        int facing = enemy != null ? enemy.FacingDir : -1;
        Vector3 forwardDir = (facing >= 0) ? Vector3.right : Vector3.left;

        // 정면 레이
        Debug.DrawRay(
            transform.position,
            forwardDir * viewRadius,
            Color.red
        );

        // 시야각 양쪽 경계
        Vector3 left = DirFromAngle(-viewAngle * 0.5f, forwardDir);
        Vector3 right = DirFromAngle(viewAngle * 0.5f, forwardDir);

        Debug.DrawRay(transform.position, left * viewRadius, Color.yellow);
        Debug.DrawRay(transform.position, right * viewRadius, Color.yellow);
    }
}
