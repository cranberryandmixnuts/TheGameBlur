using System.Collections.Generic;
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
}
