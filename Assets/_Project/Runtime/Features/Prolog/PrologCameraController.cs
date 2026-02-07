using UnityEngine;

public class PrologCameraController : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float followSpeed;

    private bool isActive = true;

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
            .LerpTo(followTarget.position + offset, followSpeed)
            .ToVector2()
            .ToVector3(-15f);
    }
}
