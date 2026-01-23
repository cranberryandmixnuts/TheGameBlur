using UnityEngine;

public class SignalDialogPlayer : MonoBehaviour
{
    public void PlayDialog(DialogData dialogData)
    {
        CinematicManager.Show<CinematicDialog>().BindDialog(dialogData);
    }
}
