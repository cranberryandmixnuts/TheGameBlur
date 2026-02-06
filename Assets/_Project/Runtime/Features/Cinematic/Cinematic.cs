using System;
using UnityEngine;

public abstract class Cinematic : MonoBehaviour
{
    public event Action<Cinematic> OnFinished;

    public virtual void Play() { }

    public virtual void Finish()
    {
        OnFinished?.Invoke(this);
    }
}
