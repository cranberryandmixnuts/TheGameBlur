using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.LoadScene(SceneType.TitleScene);
        }
    }
}
