using UnityEngine;

public class FallCameraController : MonoBehaviour
{
    [SerializeField] private float strength = 1f;
    [SerializeField] private float vibrato = 10f;

    public void FallShake()
    {
        GetComponent<CameraShaker>().Shake(10, strength, vibrato, false);
    }
}
