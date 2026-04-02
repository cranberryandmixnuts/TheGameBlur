using System;
using UnityEngine;

public class CinematicManager : MonoBehaviour
{
    public static CinematicManager Instance {  get; private set; }
    public static event Action<Cinematic> OnFinished;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod]
    private static void GenerateSceneController()
    {
        GameObject cinematicManager = new GameObject(typeof(CinematicManager).Name);
        cinematicManager.AddComponent<CinematicManager>();
    }
#endif

    [SerializeField] private CinematicRegistry cinematicRegistry;

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

    public static T Show<T>() where T : Cinematic
    {
        var entry = Instance.cinematicRegistry.GetEntry<T>();
        if (entry != null)
        {
            T cinematic = Instantiate(entry.Cinematic) as T;
            cinematic.OnFinished += Instance.HandleCinematicFinished;

            return cinematic;
        }
        else
        {
            return null;
        }
    }

    private void HandleCinematicFinished(Cinematic cinematic)
    {
        cinematic.OnFinished -= HandleCinematicFinished;

        OnFinished?.Invoke(cinematic);
    }
}
