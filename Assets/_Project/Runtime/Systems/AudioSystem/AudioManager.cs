using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioRegistry audioRegistry;

    [SerializeField] private AudioSource sfxPrefab;
    [SerializeField] private AudioSource bgmPrefab;

    private List<AudioSource> sfxSourcePool = new();
    private LinkedList<AudioSource> bgmSourcePool = new();

    private static float sfxVolume = 1f;
    private static float bgmVolume = 1f;

    public static float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = value;
        }
    }

    public static float BGMVolume
    {
        get => bgmVolume;
        set
        {
            bgmVolume = value;
            Instance.bgmSourcePool.First.Value.volume = value;
        }
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod]
    private static void GenerateAudioManager()
    {
        GameObject audioManager = new GameObject(typeof(AudioManager).Name);
        audioManager.AddComponent<AudioManager>();
    }
#endif

    public static AudioManager Instance { get; private set; }

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

    public void SetBGM(string audioName, float fadeDuration = 1f)
    {
        if (bgmSourcePool.Count <= 0) bgmSourcePool.AddFirst(Instantiate(bgmPrefab, transform));

        bgmSourcePool.First.Value.DOKill();
        bgmSourcePool.First.Value.DOFade(0, fadeDuration);
        bgmSourcePool.AddLast(bgmSourcePool.First.Value);
        bgmSourcePool.RemoveFirst();

        if (!Mathf.Approximately(bgmSourcePool.First.Value.volume, 0))
            bgmSourcePool.AddFirst(Instantiate(bgmPrefab, transform));

        AudioSource audioSource = bgmSourcePool.First.Value;

        audioSource.clip = audioRegistry.GetAudioClip(audioName);
        audioSource.Play();
        audioSource.volume = 0;
        audioSource.DOKill();
        audioSource.DOFade(bgmVolume, fadeDuration);
    }

    public void PlaySFX(string audioName)
    {
        AudioSource audioSource = GetAvailableSFXSource();
        audioSource.clip = audioRegistry.GetAudioClip(audioName);
        audioSource.volume = sfxVolume;
        audioSource.Play();
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var sfxSource in sfxSourcePool)
        {
            if (!sfxSource.isPlaying)
                return sfxSource;
        }

        AudioSource newSource = Instantiate(sfxPrefab, transform);
        sfxSourcePool.Add(newSource);

        return newSource;
    }
}
