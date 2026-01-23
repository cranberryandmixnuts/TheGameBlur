using UnityEngine;

public class PrologCameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float followSpeed;

    private bool isActive = false;

    public void ActiveCamera()
    {
        isActive = true;
    }

    public void InactiveCamera()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive)
            return;

        transform.position = transform.position
            .LerpTo(playerTransform.position + offset, followSpeed)
            .ToVector2()
            .ToVector3(transform.position.z);
    }
}
