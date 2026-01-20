using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class CameraBounds : MonoBehaviour
{
    private BoxCollider box;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    public Vector3 Clamp(Vector3 desired, Camera cam)
    {
        Bounds b = box.bounds;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float minX = b.min.x + halfW;
        float maxX = b.max.x - halfW;

        float minY = b.min.y + halfH;
        float maxY = b.max.y - halfH;

        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Clamp(desired.y, minY, maxY);

        return desired;
    }
}
