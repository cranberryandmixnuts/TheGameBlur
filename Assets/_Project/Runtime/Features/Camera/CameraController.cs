using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float speed;

    [SerializeField] private bool isActive = false;

    private CameraShaker cameraShaker;

    private void Awake()
    {
        Instance = this;

        if(!TryGetComponent(out cameraShaker))
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

    private void Update()
    {
        if (!isActive)
            return;

        var target = Vector2.Lerp(
            transform.position, 
            new Vector2(playerTransform.position.x, playerTransform.position.y) + offset, 
            speed * Time.deltaTime);
        transform.position = new Vector3(target.x, target.y, transform.position.z);
    }
}
