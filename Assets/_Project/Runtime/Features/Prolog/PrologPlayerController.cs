using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

public class PrologPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    
    private Transform followTarget = null;
    private Rigidbody _rigidBody;
    private bool isActive = false;

    public void ActivePlayer()
    {
        _rigidBody = GetComponent<Rigidbody>();
        isActive = true;
        
    }

    public void InactivePlayer()
    {
        isActive = false;
    }

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    private void Update()
    {
        if (!isActive && followTarget != null && followTarget != transform)
        {
            //Debug.Log(followTarget.gameObject.name);
            transform.position = transform.position
                .LerpTo(followTarget.position, 0.1f)
                .ToVector2()
                .ToVector3(transform.position.z);
        }
        if (!isActive)
            return;

        float moveRate = Input.GetAxisRaw("Horizontal");

        _rigidBody.linearVelocity = Vector3.right * moveRate * moveSpeed;
    }
}
