using UnityEngine;

public class BGMSetter : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;

    private void Start()
    {
        AudioManager.Instance.SetBGM(audioClip.name);
    }
}
