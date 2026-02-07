using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    private Vector3 lastOffset;

    private float time;
    private float remaining;
    private float duration;
    private float currentStrength;
    private float targetStrength;
    private float frequency;
    private float randomness;
    private bool fadeOut;

    public void Shake()
    {
        Shake(0.5f, 1f, 1);
    }

    public void Shake(float duration, float strength, int vibrato = 10, int randomness = 90, bool fadeOut = true)
    {
        this.duration = Mathf.Max(this.duration, duration);
        remaining = Mathf.Max(remaining, duration);
        targetStrength = strength;
        frequency = Mathf.Max(1f, vibrato) * 2f;
        this.randomness = Mathf.Clamp01(randomness / 180f);
        this.fadeOut = fadeOut;
    }

    private void LateUpdate()
    {
        transform.position -= lastOffset;

        if (remaining <= 0f)
        {
            currentStrength = Mathf.MoveTowards(currentStrength, 0f, Time.deltaTime * 10f);
            lastOffset = Vector3.zero;
            return;
        }

        remaining -= Time.deltaTime;

        float fadeMultiplier = 1f;
        if (fadeOut)
            fadeMultiplier = Mathf.Clamp01(remaining / duration);

        currentStrength = Mathf.MoveTowards(currentStrength, targetStrength, Time.deltaTime * 10f);

        time += Time.deltaTime * frequency;

        float nx = Mathf.PerlinNoise(time, 0f);
        float ny = Mathf.PerlinNoise(0f, time);

        float x = Mathf.Lerp(nx, Random.value, randomness) * 2f - 1f;
        float y = Mathf.Lerp(ny, Random.value, randomness) * 2f - 1f;

        lastOffset = new Vector3(x, y, 0f) * currentStrength * fadeMultiplier;

        transform.position += lastOffset;
    }
}
