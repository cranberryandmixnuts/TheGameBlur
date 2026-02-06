using System;
using UnityEngine;

public class SettingView : UIView
{
    public event Action OnQuit;
    public event Action OnContinue;
    public event Action<float> OnSetBGMVolume;
    public event Action<float> OnSetSFXVolume;

    public void Quit()
    {
        OnQuit?.Invoke();
    }

    public void Continue()
    {
        Destroy(gameObject);
        OnContinue?.Invoke();
    }

    public void SetBGMVolume(float volume)
    {
        OnSetBGMVolume?.Invoke(volume);
    }

    public void SetSFXVolume(float volume)
    {
        OnSetSFXVolume?.Invoke(volume);
    }
}
