using UnityEngine;

public sealed class PlayerAttackRangeIndicator : MonoBehaviour
{
    [SerializeField] private LineRenderer groundCircle;
    [SerializeField] private LineRenderer airSector;

    private PlayerSettings settings;
    private float remaining;

    private void Awake()
    {
        settings = Player.Instance.Settings;

        if (groundCircle == null) groundCircle = CreateLineRenderer("GroundAttackRange");
        if (airSector == null) airSector = CreateLineRenderer("AirAttackRange");

        groundCircle.enabled = false;
        airSector.enabled = false;
    }

    private void Update()
    {
        if (remaining <= 0f) return;

        remaining -= Time.deltaTime;
        if (remaining > 0f) return;

        remaining = 0f;
        groundCircle.enabled = false;
        airSector.enabled = false;
    }

    public void ShowGroundCircle(Vector3 center, float radius)
    {
        if (!settings.showAttackRangeOnAttack) return;

        center.z = settings.planeZ;

        int seg = Mathf.Max(8, settings.attackRangeSegments);
        EnsureCircle(groundCircle, center, radius, seg);

        groundCircle.startWidth = settings.attackRangeLineWidth;
        groundCircle.endWidth = settings.attackRangeLineWidth;

        groundCircle.enabled = true;
        airSector.enabled = false;

        remaining = settings.attackRangeVisualDuration;
    }

    public void ShowAirSector(Vector3 origin, float radius, float halfAngleDeg, int sign)
    {
        if (!settings.showAttackRangeOnAttack) return;

        origin.z = settings.planeZ;

        int seg = Mathf.Max(6, settings.attackRangeSegments);
        EnsureSector(airSector, origin, radius, halfAngleDeg, sign, seg);

        airSector.startWidth = settings.attackRangeLineWidth;
        airSector.endWidth = settings.attackRangeLineWidth;

        airSector.enabled = true;
        groundCircle.enabled = false;

        remaining = settings.attackRangeVisualDuration;
    }

    private static void EnsureCircle(LineRenderer lr, Vector3 center, float radius, int segments)
    {
        lr.loop = true;
        lr.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            float a = t * Mathf.PI * 2f;

            float x = Mathf.Cos(a) * radius;
            float y = Mathf.Sin(a) * radius;

            lr.SetPosition(i, center + new Vector3(x, y, 0f));
        }
    }

    private static void EnsureSector(LineRenderer lr, Vector3 origin, float radius, float halfAngleDeg, int sign, int segments)
    {
        lr.loop = false;

        int arcCount = segments + 1;
        int total = 1 + arcCount + 1;

        lr.positionCount = total;

        lr.SetPosition(0, origin);

        float step = (halfAngleDeg * 2f) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float a = -halfAngleDeg + step * i;
            float rad = a * Mathf.Deg2Rad;

            float x = Mathf.Cos(rad) * sign;
            float y = Mathf.Sin(rad);

            Vector3 p = origin + new Vector3(x, y, 0f) * radius;
            lr.SetPosition(1 + i, p);
        }

        lr.SetPosition(total - 1, origin);
    }

    private LineRenderer CreateLineRenderer(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.enabled = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        Shader shader = Shader.Find("Sprites/Default");
        lr.material = new Material(shader);

        return lr;
    }
}