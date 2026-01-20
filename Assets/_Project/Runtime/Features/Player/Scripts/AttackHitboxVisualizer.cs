using UnityEngine;

public sealed class AttackHitboxVisualizer : MonoBehaviour
{
    [SerializeField] private float lineWidth = 0.03f;

    private LineRenderer bottom;
    private LineRenderer top;
    private LineRenderer v0;
    private LineRenderer v1;
    private LineRenderer v2;
    private LineRenderer v3;

    private float hideAt;

    private void Awake()
    {
        Material mat = new(Shader.Find("Sprites/Default"));

        bottom = CreateLoop("Bottom", mat);
        top = CreateLoop("Top", mat);

        v0 = CreateLine("V0", mat);
        v1 = CreateLine("V1", mat);
        v2 = CreateLine("V2", mat);
        v3 = CreateLine("V3", mat);

        SetEnabled(false);
    }

    public void Show(Vector3 center, Vector3 halfExtents, Quaternion rotation, float seconds)
    {
        transform.SetPositionAndRotation(center, rotation);
        transform.localScale = halfExtents * 2f;

        ConfigureUnitCube();

        hideAt = Time.time + seconds;
        SetEnabled(true);
    }

    private void Update()
    {
        if (Time.time >= hideAt)
            SetEnabled(false);
    }

    private void ConfigureUnitCube()
    {
        Vector3 b0 = new(-0.5f, -0.5f, -0.5f);
        Vector3 b1 = new(0.5f, -0.5f, -0.5f);
        Vector3 b2 = new(0.5f, 0.5f, -0.5f);
        Vector3 b3 = new(-0.5f, 0.5f, -0.5f);

        Vector3 t0 = new(-0.5f, -0.5f, 0.5f);
        Vector3 t1 = new(0.5f, -0.5f, 0.5f);
        Vector3 t2 = new(0.5f, 0.5f, 0.5f);
        Vector3 t3 = new(-0.5f, 0.5f, 0.5f);

        bottom.positionCount = 5;
        bottom.SetPosition(0, b0);
        bottom.SetPosition(1, b1);
        bottom.SetPosition(2, b2);
        bottom.SetPosition(3, b3);
        bottom.SetPosition(4, b0);

        top.positionCount = 5;
        top.SetPosition(0, t0);
        top.SetPosition(1, t1);
        top.SetPosition(2, t2);
        top.SetPosition(3, t3);
        top.SetPosition(4, t0);

        SetVertical(v0, b0, t0);
        SetVertical(v1, b1, t1);
        SetVertical(v2, b2, t2);
        SetVertical(v3, b3, t3);
    }

    private void SetVertical(LineRenderer lr, Vector3 a, Vector3 b)
    {
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
    }

    private LineRenderer CreateLoop(string name, Material mat)
    {
        GameObject go = new(name);
        go.transform.SetParent(transform, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = false;
        lr.sharedMaterial = mat;
        lr.widthMultiplier = lineWidth;

        return lr;
    }

    private LineRenderer CreateLine(string name, Material mat)
    {
        GameObject go = new(name);
        go.transform.SetParent(transform, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = false;
        lr.sharedMaterial = mat;
        lr.widthMultiplier = lineWidth;

        return lr;
    }

    private void SetEnabled(bool enabled)
    {
        bottom.enabled = enabled;
        top.enabled = enabled;

        v0.enabled = enabled;
        v1.enabled = enabled;
        v2.enabled = enabled;
        v3.enabled = enabled;
    }
}