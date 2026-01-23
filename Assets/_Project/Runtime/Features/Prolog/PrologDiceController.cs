using UnityEngine;

public class PrologDiceController : MonoBehaviour
{
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
        if(Input.GetKeyDown(KeyCode.E) && isEnter)
        {
            Destroy(this);
            CinematicManager.Show<CinematicGoldBug>();
            if (interactionView != null) Destroy(interactionView.gameObject);
        }
    }
}
