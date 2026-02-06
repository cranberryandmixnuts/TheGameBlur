using UnityEngine;

public class MovementLock : MonoBehaviour
{
    [SerializeField] private Transform[] lockTargets;

    private void LateUpdate()
    {
        foreach(Transform targetTransform in lockTargets)
        {
            targetTransform.position = transform.position;
            targetTransform.rotation = transform.rotation;
        }
    }
}
