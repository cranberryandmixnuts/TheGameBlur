using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneType sceneType;

    private void Awake()
    {
        
    }

    public void LoadScene()
    {
        SceneController.Instance.LoadScene(sceneType);
    }
}
