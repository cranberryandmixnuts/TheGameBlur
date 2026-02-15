using System.Collections.Generic;
using UnityEngine;

public class CombatSceneMover : MonoBehaviour
{
    [Header("Scene Setting")]
    [SerializeField] private SceneType loadSceneType;

    [Header("Interact Setting")]
    [SerializeField] private InteractionView interactionViewPrefab;
    [SerializeField] private Vector2 offset;


    private List<InteractionView> interactionViews = new();
    private bool isCleared = false;

    public void OnClear()
    {
        isCleared = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCleared)
            return;

        interactionViews.Add(Instantiate(interactionViewPrefab));
        interactionViews[interactionViews.Count - 1].SetInteraction(transform, offset);
    }

    private void OnTriggerExit(Collider other)
    {
        if (interactionViews.Count > 0)
        {
            Destroy(interactionViews[0].gameObject);
            interactionViews.RemoveAt(0);
        }
    }

    private void Update()
    {
        if (InputManager.Instance.InteractionDown)
        {
            if (interactionViews.Count > 0)
            {
                OnInteract();
            }
        }
    }

    private void OnInteract()
    {
        foreach (InteractionView interactionView in interactionViews)
            Destroy(interactionView.gameObject);

        AudioManager.Instance.PlaySFX("Interact");

        interactionViews.Clear();

        SceneController.Instance.LoadScene(loadSceneType);
    }
}
