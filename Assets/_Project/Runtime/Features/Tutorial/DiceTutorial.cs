using UnityEngine;

public class DiceTutorial : MonoBehaviour
{
    private bool isEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isEntered)
            return;

        if (other.gameObject.TryGetComponent<Player>(out var _))
        {
            isEntered = true;
            Player.Instance.SetDiceAbilityUnlocked(true);
        }
    }
}
