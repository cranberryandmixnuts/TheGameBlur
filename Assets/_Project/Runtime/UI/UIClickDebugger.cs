using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickDebugger : MonoBehaviour
{
    private void Update()
    {
        DebugUIRaycast();
    }

    private void DebugUIRaycast()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem이 씬에 없습니다.");
            return;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count == 0)
        {
            Debug.Log("감지된 UI/오브젝트 없음");
            return;
        }

        Debug.Log($"클릭 위치: {Input.mousePosition}, 감지 개수: {results.Count}");

        for (int i = 0; i < results.Count; i++)
        {
            RaycastResult r = results[i];

            string path = GetPath(r.gameObject.transform);

            Debug.Log(
                $"[{i}] 이름: {r.gameObject.name}\n" +
                $"    전체경로: {path}\n" +
                $"    sortingLayer: {r.sortingLayer}\n" +
                $"    sortingOrder: {r.sortingOrder}\n" +
                $"    depth: {r.depth}\n" +
                $"    distance: {r.distance}\n" +
                $"    module: {r.module.GetType().Name}"
            );
        }
    }

    private string GetPath(Transform current)
    {
        string path = current.name;

        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}