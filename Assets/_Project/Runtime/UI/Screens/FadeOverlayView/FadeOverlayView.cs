using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeOverlayView : MonoBehaviour
{
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private float duration;

    private void Start()
    {
        fadeOverlay.DOColor(new Color(0, 0, 0, 0), duration);
        Destroy(gameObject, duration);
    }
}
