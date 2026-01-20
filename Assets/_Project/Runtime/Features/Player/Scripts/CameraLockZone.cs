using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CameraLockZone : MonoBehaviour
{
    [SerializeField] private Transform lockPoint;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        Vector3 p = lockPoint != null ? lockPoint.position : transform.position;
        SideScrollerCameraController.Instance.LockAt(p);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        SideScrollerCameraController.Instance.Unlock();
    }
}
