using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    private Vector3 lastOffset;

    private float time;
    private float remaining;
    private float duration;
    //private float currentStrength;
    private float targetStrength;
    private float frequency;
    private bool fadeOut;

    public void Shake()
    {
        Debug.Log("!");
        Shake(0.5f, 6f, 10f);
    }

    public void Shake(float duration, float strength, float vibrato = 10,  bool fadeOut = true)
    {
        this.duration = Mathf.Max(this.duration, duration);
        remaining = Mathf.Max(remaining, duration);
        targetStrength = strength;
        frequency = Mathf.Max(1, vibrato);
        this.fadeOut = fadeOut;
    }

    private void LateUpdate()
    {
        transform.position -= lastOffset;

        if (remaining <= 0f)
        {
            //currentStrength = Mathf.MoveTowards(currentStrength, 0f, Time.deltaTime * 10f);
            lastOffset = Vector3.zero;
            return;
        }

        remaining -= Time.deltaTime;

        float fadeMultiplier = 1f;
        if (fadeOut)
            fadeMultiplier = Mathf.Clamp01(remaining / duration);

        //currentStrength = Mathf.MoveTowards(currentStrength, targetStrength, Time.deltaTime * 10f);

        time += Time.deltaTime * frequency;

        float nx = Mathf.PerlinNoise(time, 0f) - 0.5f;
        float ny = Mathf.PerlinNoise(0f, time) - 0.5f;

        lastOffset = new Vector3(nx, ny, 0f) * targetStrength * fadeMultiplier;

        transform.position += lastOffset;
    }
}
