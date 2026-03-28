using UnityEngine;

public class PrologDiceController : MonoBehaviour
{
    [SerializeField] private PrologManager prologManager;

    private InteractionView interactionView;
    private bool isEnter = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PrologPlayerController>(out var playerController))
        {
            isEnter = true;
            interactionView = UIManager.Show<InteractionView>();
            interactionView.SetInteraction(transform, Vector3.up * 100);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PrologPlayerController>(out var playerController))
        {
            isEnter = false;
            if(interactionView != null) Destroy(interactionView.gameObject);
        }
        
    }

    private void Update()
    {
        if(InputManager.Instance.InteractionDown && isEnter)
        {
            Destroy(this);
            prologManager.StartGoldBug();
            if (interactionView != null) Destroy(interactionView.gameObject);
        }
    }
}
