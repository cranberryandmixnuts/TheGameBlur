using UnityEngine;

public class PrologPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    [SerializeField] private Transform playerModelTransform;
    [SerializeField] private Animator playerModelAnimator;

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

        float moveRate = InputManager.Instance.MoveAxis;
        _rigidBody.linearVelocity = Vector3.right * moveRate * moveSpeed;

        if (!Mathf.Approximately(moveRate, 0))
        {
            playerModelAnimator.SetBool("IsRun", true);
            float targetY = moveRate > 0f ? 90f : 270f;
            Quaternion targetRotation = Quaternion.Euler(0f, targetY, 0f);
            playerModelTransform.rotation = Quaternion.Lerp(playerModelTransform.rotation, targetRotation, 20 * Time.deltaTime);
        }
        else
        {
            if(playerModelAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "metarig|Run_Weapon")
                playerModelAnimator.SetBool("IsRun", false);
        }
    }
}
