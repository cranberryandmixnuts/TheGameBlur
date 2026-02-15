using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Interacter : MonoBehaviour
{
    [SerializeField] private InteractionView interactionViewPrefab;
    [SerializeField] private Vector2 offset;

    private List<InteractionView> interactionViews = new();

    private void OnTriggerEnter(Collider other)
    {
        interactionViews.Add(Instantiate(interactionViewPrefab));
        interactionViews[interactionViews.Count - 1].SetInteraction(transform, offset);
    }

    private void OnTriggerExit(Collider other)
    {
        if(interactionViews.Count > 0)
        {
            Destroy(interactionViews[0].gameObject);
            interactionViews.RemoveAt(0);
        }
    }

    private void Update()
    {
        if(InputManager.Instance.InteractionDown)
        {
            if(interactionViews.Count > 0)
            {
                foreach (InteractionView interactionView in interactionViews)
                    Destroy(interactionView.gameObject);

                AudioManager.Instance.PlaySFX("Interact");

                interactionViews.Clear();

            }
        }
    }
}
