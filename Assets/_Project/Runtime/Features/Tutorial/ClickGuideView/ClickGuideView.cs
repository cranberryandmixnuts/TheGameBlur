using UnityEngine;
using UnityEngine.UI;

public class ClickGuideView : MonoBehaviour
{
    [SerializeField] private RectTransform clickGuideObject;

    private CanvasScaler canvasScaler;
    private float rate;
    private Transform interactionTarget;
    private Vector3 offset;

    private bool isActive = false;

    public void SetClickGuide(Transform interactionTarget, Vector3? offset = null)
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

        clickGuideObject.anchoredPosition = screenPos * rate + offset;
    }
}
