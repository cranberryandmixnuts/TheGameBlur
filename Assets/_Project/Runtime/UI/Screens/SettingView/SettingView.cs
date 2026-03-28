using System;
using UnityEngine;

public class SettingView : UIView
{
    [SerializeField] private Animator settingAnimator;

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

    public void EnterOption()
    {
        settingAnimator.Play("EnterOption", 0, 0f);
    }

    public void ExitOption()
    {
        settingAnimator.Play("ExitOption", 0, 0f);
    }
}
