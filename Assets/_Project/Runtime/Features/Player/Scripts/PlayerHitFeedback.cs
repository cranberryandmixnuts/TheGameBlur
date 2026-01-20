using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public sealed class PlayerHitFeedback : MonoBehaviour
{
    [SerializeField] private PlayerSettings settings;
    [SerializeField] private ScreenDarkenOverlay darkenOverlay;

    private PlayerHealth health;
    private Coroutine routine;

    private float originalFixedDeltaTime;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        health.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        health.OnDamaged -= OnDamaged;
    }

    private void OnDamaged(DamagePayload payload)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Co());
    }

    private IEnumerator Co()
    {
        if (darkenOverlay != null)
            darkenOverlay.SetAlpha(settings.darkenAlpha);

        SideScrollerCameraController.Instance.AddShake(settings.shakeDuration, settings.shakeAmplitude, settings.shakeFrequency);

        float prevScale = Time.timeScale;

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        yield return new WaitForSecondsRealtime(settings.hitStopDuration);

        Time.timeScale = prevScale <= 0f ? 1f : prevScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        if (darkenOverlay != null)
            darkenOverlay.SetAlpha(0f);

        routine = null;
    }
}
