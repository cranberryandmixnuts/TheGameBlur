using UnityEngine;

public class CombatClearNotifier : MonoBehaviour
{
    [SerializeField] private CombatSceneMover sceneMover;

    private void OnDestroy()
    {
        sceneMover.OnClear();
    }
}
