using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class ScreenDarkenOverlay : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void SetAlpha(float a) => canvasGroup.alpha = a;
}
