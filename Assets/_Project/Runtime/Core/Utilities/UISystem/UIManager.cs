using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod]
    private static void GenerateSceneController()
    {
        GameObject uiManager = new GameObject(typeof(UIManager).Name);
        uiManager.AddComponent<UIManager>();
    }
#endif

    [SerializeField] private UIRegistry uiRegistry;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static T Show<T>() where T : UIView
    {
        var entry = Instance.uiRegistry.GetEntry<T>();
        if (entry != null)
        {
            return Instantiate(entry.Prefab) as T;
        }
        else
        {
            return null;
        }
    }
}