using UnityEngine;

public sealed class PlayerAttackRangeIndicator : MonoBehaviour
{
    [SerializeField] private LineRenderer groundCircle;
    [SerializeField] private LineRenderer airCircle;

    [Header("Ultimate Box")]
    [SerializeField] private LineRenderer ultimateRect;

    private PlayerSettings settings;
    private float remaining;

    private void Awake()
    {
        settings = Player.Instance.Settings;

        if (groundCircle == null) groundCircle = CreateLineRenderer("GroundAttackRange");
        if (airCircle == null) airCircle = CreateLineRenderer("AirAttackRange");
        if (ultimateRect == null) ultimateRect = CreateLineRenderer("UltimateBoxRange");

        DisableAll();
    }

    private void Update()
    {
        if (remaining <= 0f) return;

        remaining -= Time.deltaTime;
        if (remaining > 0f) return;

        remaining = 0f;
        DisableAll();
    }

    public void ShowGroundCircle(Vector3 center, float radius) => ShowGroundCircle(center, radius, settings.attackRangeVisualDuration);

    public void ShowGroundCircle(Vector3 center, float radius, float duration)
    {
        if (!settings.showAttackRangeOnAttack) return;

        center.z = settings.planeZ;

        int seg = Mathf.Max(8, settings.attackRangeSegments);
        EnsureCircle(groundCircle, center, radius, seg);

        groundCircle.startWidth = settings.attackRangeLineWidth;
        groundCircle.endWidth = settings.attackRangeLineWidth;

        groundCircle.enabled = true;
        airCircle.enabled = false;
        ultimateRect.enabled = false;

        remaining = duration;
    }

    public void ShowAirCircle(Vector3 center, float radius) => ShowAirCircle(center, radius, settings.attackRangeVisualDuration);

    public void ShowAirCircle(Vector3 center, float radius, float duration)
    {
        if (!settings.showAttackRangeOnAttack) return;

        center.z = settings.planeZ;

        int seg = Mathf.Max(8, settings.attackRangeSegments);
        EnsureCircle(airCircle, center, radius, seg);

        airCircle.startWidth = settings.attackRangeLineWidth;
        airCircle.endWidth = settings.attackRangeLineWidth;

        airCircle.enabled = true;
        groundCircle.enabled = false;
        ultimateRect.enabled = false;

        remaining = duration;
    }

    public void ShowUltimateBox(Vector3 center, Vector3 size, float duration)
    {
        if (!settings.showAttackRangeOnAttack) return;

        center.z = settings.planeZ;

        Vector2 half = new Vector2(size.x * 0.5f, size.y * 0.5f);
        EnsureRect(ultimateRect, center, half);

        ultimateRect.startWidth = settings.attackRangeLineWidth;
        ultimateRect.endWidth = settings.attackRangeLineWidth;

        ultimateRect.enabled = true;
        groundCircle.enabled = false;
        airCircle.enabled = false;

        remaining = duration;
    }

    private void DisableAll()
    {
        groundCircle.enabled = false;
        airCircle.enabled = false;
        ultimateRect.enabled = false;
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

    private static void EnsureRect(LineRenderer lr, Vector3 center, Vector2 halfExtents)
    {
        lr.loop = true;
        lr.positionCount = 4;

        Vector3 a = new Vector3(center.x - halfExtents.x, center.y - halfExtents.y, center.z);
        Vector3 b = new Vector3(center.x - halfExtents.x, center.y + halfExtents.y, center.z);
        Vector3 c = new Vector3(center.x + halfExtents.x, center.y + halfExtents.y, center.z);
        Vector3 d = new Vector3(center.x + halfExtents.x, center.y - halfExtents.y, center.z);

        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        lr.SetPosition(2, c);
        lr.SetPosition(3, d);
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