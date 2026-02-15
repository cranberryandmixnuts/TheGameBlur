using UnityEngine;

public class FallManager : MonoBehaviour
{
    public void OnExitFallActionTimeline()
    {
        SceneController.Instance.LoadScene(SceneType.TutorialScene);
    }

    private void Start()
    {
        AudioManager.Instance.SetBGM(null);
    }
}
