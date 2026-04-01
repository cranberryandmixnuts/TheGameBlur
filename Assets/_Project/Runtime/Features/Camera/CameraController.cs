using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private bool isActive = false;

    private CameraShaker cameraShaker;

    private void Awake()
    {
        Instance = this;

        if (!TryGetComponent(out cameraShaker))
            cameraShaker = gameObject.AddComponent<CameraShaker>();
    }

    private void Start()
    {
        FindAnyObjectByType<PlayerCombat>().OnAttacked += OnAttacked;
        FindAnyObjectByType<PlayerStats>().HpChanged += OnHpChanged;
    }

    private void OnAttacked()
    {
        cameraShaker.Shake(0.5f, .75f, 10f);
    }

    private void OnHpChanged(int now, int max)
    {
        cameraShaker.Shake(0.5f, 1.5f, 10f);
    }

    public void Active()
    {
        isActive = true;
    }

    public void Inactive()
    {
        isActive = false;
    }
}
