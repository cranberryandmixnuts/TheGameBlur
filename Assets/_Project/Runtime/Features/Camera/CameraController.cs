using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float speed;

    [SerializeField] private bool isActive = false;

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
