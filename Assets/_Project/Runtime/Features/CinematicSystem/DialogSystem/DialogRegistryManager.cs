using System;
using UnityEngine;

public class DialogRegistryManager : MonoBehaviour
{
    public static DialogRegistryManager Instance { get; private set; }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod]
    private static void GenerateSceneController()
    {
        GameObject dialogRegistry = new GameObject(typeof(DialogRegistryManager).Name);
        dialogRegistry.AddComponent<DialogRegistryManager>();
    }
#endif

    [SerializeField] private DialogRegistry dialogRegistry;

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

    public DialogData GetDialogData(string name) =>
        dialogRegistry.GetDialogData(name);
}
