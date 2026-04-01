using UnityEngine;

public class HealthPotionTutorial : MonoBehaviour
{
    private bool isFirstStandUp = true;

    private void Start()
    {
        Player.Instance.OnPlayerStandUp += OnPlayerStandUp;
    }

    private void OnPlayerStandUp()
    {
        if (!isFirstStandUp)
            return;

        isFirstStandUp = false;
        CinematicManager.Show<CinematicDialog>().BindDialog("HealthPotionTutorial");
    }
}
