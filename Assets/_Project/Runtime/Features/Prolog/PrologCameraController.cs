using UnityEngine;

public class PrologCameraController : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float followSpeed;

    public void SetTarget(Transform target)
    {
        followTarget = target;
    }

    public void SetFollowSpeed(float followSpeed)
    {
        this.followSpeed = followSpeed;
    }

    public void SetOffsetZero()
    {
        this.offset = Vector3.zero;
    }

    private void Update()
    {
        transform.position = transform.position
            .LerpTo(followTarget.position + offset, followSpeed)
            .ToVector2()
            .ToVector3(transform.position.z);
    }
}
