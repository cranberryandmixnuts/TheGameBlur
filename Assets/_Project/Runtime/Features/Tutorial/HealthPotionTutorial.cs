using UnityEngine;

public class HealthPotionTutorial : MonoBehaviour
{
    private bool isFirstStandUp = true;

    private void Start()
    {
        Player.Instance.OnPlayerStandUp += OnPlayerStandUp;
        Player.Instance.SetPotionAbilityUnlocked(true);
    }

    private void OnPlayerStandUp()
    {
        if (!isFirstStandUp)
            return;

        isFirstStandUp = false;
        CinematicManager.Show<CinematicDialog>().BindDialog("HealthPotionTutorial");
    }
}
