using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioRegistry", menuName = "Scriptable Objects/AudioRegistry")]
public class AudioRegistry : ScriptableObject
{
    [SerializeField] private List<AudioClip> audioClips;

    private Dictionary<string, AudioClip> cache;

    private void OnEnable()
    {
        cache = new Dictionary<string, AudioClip>();

        foreach (var clip in audioClips)
        {
            if (clip == null)
                continue;

            cache[clip.name] = clip;
        }
    }

    public AudioClip GetAudioClip(string audioName)
    {
        try
        {
            cache.TryGetValue(audioName, out var clip);

            return clip;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return null;
        }
    }
}
