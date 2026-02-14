using UnityEngine;

public sealed class BossRoomTrigger : MonoBehaviour
{
    public BossController boss;
    public bool deactivateOnExit = true;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (boss == null) return;
        if (!other.CompareTag("Player")) return;

        boss.ActivateBoss(other.transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (!deactivateOnExit) return;
        if (boss == null) return;
        if (!other.CompareTag("Player")) return;

        boss.DeactivateBoss();
    }
}
