using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class CameraBounds : MonoBehaviour
{
    [SerializeField] private BoxCollider box;

    public bool TryGetAllowedRect(Camera cam, out Rect rect)
    {
        Bounds b = box.bounds;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float minX = b.min.x + halfW;
        float maxX = b.max.x - halfW;
        float minY = b.min.y + halfH;
        float maxY = b.max.y - halfH;

        if (minX > maxX || minY > maxY)
        {
            rect = default;
            return false;
        }

        rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
        return true;
    }

    public bool Contains(Vector3 position, Camera cam)
    {
        if (!TryGetAllowedRect(cam, out Rect rect))
            return false;

        return rect.Contains(new Vector2(position.x, position.y));
    }

    public Vector3 Clamp(Vector3 desired, Camera cam)
    {
        if (!TryGetAllowedRect(cam, out Rect rect))
            return desired;

        desired.x = Mathf.Clamp(desired.x, rect.xMin, rect.xMax);
        desired.y = Mathf.Clamp(desired.y, rect.yMin, rect.yMax);

        return desired;
    }
}