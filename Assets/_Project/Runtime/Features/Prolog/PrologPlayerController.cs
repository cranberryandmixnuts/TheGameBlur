using UnityEngine;

public class PrologPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

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

    private void Update()
    {
        if (!isActive)
            return;

        float moveRate = Input.GetAxisRaw("Horizontal");

        _rigidBody.linearVelocity = Vector3.right * moveRate * moveSpeed;
    }
}
