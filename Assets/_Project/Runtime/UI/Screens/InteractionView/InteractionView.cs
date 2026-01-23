using UnityEngine;
using UnityEngine.UI;

public class InteractionView : UIView
{
    [SerializeField] private RectTransform interactionObject;

    private CanvasScaler canvasScaler;
    private float rate;
    private Transform interactionTarget;
    private Vector3 offset;

    private bool isActive = false;

    public void SetInteraction(Transform interactionTarget, Vector3? offset = null)
    {
        canvasScaler = GetComponent<CanvasScaler>();

        this.offset = offset ?? Vector3.zero;
        this.interactionTarget = interactionTarget;
        
        isActive = true;
    }

    private void Update()
    {
        if (!isActive)
            return;

        var screenPos = Camera.main.WorldToScreenPoint(interactionTarget.position);
        rate = canvasScaler.referenceResolution.x / Screen.width;

        interactionObject.anchoredPosition = screenPos * rate + offset;
    }
}
