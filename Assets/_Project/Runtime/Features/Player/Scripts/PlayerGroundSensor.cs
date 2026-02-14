using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerGroundSensor : MonoBehaviour
{
    public bool IsTouchingGround => groundColliders.Count > 0;

    private LayerMask groundMask;
    private Transform playerRoot;
    private readonly HashSet<Collider> groundColliders = new HashSet<Collider>();

    public void Initialize(LayerMask mask, Transform root)
    {
        groundMask = mask;
        playerRoot = root;
    }

    private void OnDisable() => groundColliders.Clear();

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (playerRoot != null && other.transform.IsChildOf(playerRoot)) return;
        if (((1 << other.gameObject.layer) & groundMask.value) == 0) return;

        groundColliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;
        if (((1 << other.gameObject.layer) & groundMask.value) == 0) return;

        groundColliders.Remove(other);
    }
}