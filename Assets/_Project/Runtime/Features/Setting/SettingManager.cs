using UnityEngine;

public class SettingManager : MonoBehaviour
{
    private SettingView settingView = null;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod]
    private static void GenerateSettingManager()
    {
        GameObject settingManager = new GameObject(typeof(SettingManager).Name);
        settingManager.AddComponent<SettingManager>();
    }
#endif

    public static SettingManager Instance { get; private set; }

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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(settingView != null)
            {
                settingView.Continue();
            }
            else
                GenerateSettingView();
        }
    }

    private void GenerateSettingView()
    {
        settingView = UIManager.Show<SettingView>();

        Time.timeScale = 0f;

        settingView.OnQuit += OnQuit;
        settingView.OnContinue += OnContinue;
        settingView.OnSetBGMVolume += SetBGMVolume;
        settingView.OnSetSFXVolume += SetSFXVolume;
    }

    private void OnQuit()
    {
        UIManager.Show<QuitGameView>();
    }

    private void OnContinue()
    {
        Time.timeScale = 1f;
    }

    private void SetBGMVolume(float volume)
    {
        AudioManager.SFXVolume = volume;
    }

    private void SetSFXVolume(float volume)
    {
        AudioManager.BGMVolume = volume;
    }
}
