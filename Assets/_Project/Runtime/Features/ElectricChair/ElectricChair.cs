using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ElectricChair : MonoBehaviour
{
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Vector3 seatOffset;

    private Player player;
    private bool playerInRange;

    private void Awake()
    {
        Collider c = GetComponent<Collider>();
        c.isTrigger = true;

        player = Player.Instance;

        if (seatPoint == null) seatPoint = transform;
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (player.IsSitting) return;

        if (player.Input.InteractionDown) SitPlayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Player>() == player)
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Player>() == player)
            playerInRange = false;
    }

    private void SitPlayer()
    {
        Vector3 p = seatPoint.position + seatOffset;
        player.Sit(this, p);
    }
}