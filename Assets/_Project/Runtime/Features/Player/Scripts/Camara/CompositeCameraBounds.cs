using System.Collections.Generic;
using UnityEngine;

public sealed class CompositeCameraBounds : MonoBehaviour
{
    [SerializeField] private List<CameraBounds> boundsList = new();

    public Vector3 Clamp(Vector3 desired, Camera cam)
    {
        bool hasValidBounds = false;
        bool insideAny = false;

        Vector3 closestPoint = desired;
        float closestDistSq = float.MaxValue;

        for (int i = 0; i < boundsList.Count; i++)
        {
            CameraBounds bounds = boundsList[i];
            if (bounds == null)
                continue;

            if (!bounds.TryGetAllowedRect(cam, out Rect rect))
                continue;

            hasValidBounds = true;

            Vector2 desired2D = new(desired.x, desired.y);
            if (rect.Contains(desired2D))
            {
                insideAny = true;
                break;
            }

            float clampedX = Mathf.Clamp(desired.x, rect.xMin, rect.xMax);
            float clampedY = Mathf.Clamp(desired.y, rect.yMin, rect.yMax);

            float dx = desired.x - clampedX;
            float dy = desired.y - clampedY;
            float distSq = dx * dx + dy * dy;

            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closestPoint = new Vector3(clampedX, clampedY, desired.z);
            }
        }

        if (!hasValidBounds)
            return desired;

        if (insideAny)
            return desired;

        return closestPoint;
    }

    private void Reset()
    {
        CollectChildren();
    }

    [ContextMenu("Collect Children")]
    public void CollectChildren()
    {
        boundsList.Clear();
        GetComponentsInChildren(true, boundsList);
        boundsList.RemoveAll(x => x == null || x.gameObject == gameObject);
    }
}