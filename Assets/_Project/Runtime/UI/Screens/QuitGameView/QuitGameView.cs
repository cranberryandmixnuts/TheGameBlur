using UnityEngine;

public class QuitGameView : UIView
{
    public void QuitGame()
    {
        Application.Quit();
    }

    public void Cancel()
    {
        Destroy(gameObject);
    }
}
