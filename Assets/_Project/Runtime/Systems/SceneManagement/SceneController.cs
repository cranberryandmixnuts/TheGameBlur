using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod]
    private static void GenerateSceneController()
    {
        GameObject sceneController = new GameObject(typeof(SceneController).Name);
        sceneController.AddComponent<SceneController>();
    }
#endif

    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;

    }

    private void Start()
    {

    }

    public void LoadScene(SceneType sceneType, float duration = 1f)
    {
        StartCoroutine(DelayedLoadScene(sceneType, duration));
    }

    private IEnumerator DelayedLoadScene(SceneType sceneType, float duration)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene(sceneType.ToString());
    }
}
