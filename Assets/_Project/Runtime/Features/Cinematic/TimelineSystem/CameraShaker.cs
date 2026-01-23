using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShaker : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private float strength;
    [SerializeField] private int vibrato;

    public void ShakePosition()
    {
        GetComponent<Camera>().DOShakePosition(duration, strength, vibrato);
    }

    public void ShakeRotation()
    {
        GetComponent<Camera>().DOShakeRotation(duration, strength, vibrato);
    }
}
