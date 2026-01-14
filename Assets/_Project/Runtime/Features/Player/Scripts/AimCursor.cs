using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
public sealed class AimCursor : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private float planeZ = 0f;

    private RectTransform rectTransform;
    private Vector3 cachedWorldPoint;

    public Vector3 WorldPosition => cachedWorldPoint;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    private void Update()
    {
        Vector2 screen = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
        rectTransform.anchoredPosition = local;

        Ray ray = viewCamera.ScreenPointToRay(screen);
        float t = (planeZ - ray.origin.z) / ray.direction.z;
        Vector3 hit = ray.origin + ray.direction * t;

        hit.z = planeZ;
        cachedWorldPoint = hit;
    }

    public Vector3 GetAimDir(Vector3 origin)
    {
        Vector3 d = cachedWorldPoint - origin;
        d.z = 0f;

        if (d.sqrMagnitude < 0.0001f)
            return Vector3.right;

        return d.normalized;
    }
}