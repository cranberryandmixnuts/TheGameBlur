using UnityEngine;

public class BGMSetter : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;

    private void Start()
    {
        if(audioClip == null)
        {
            AudioManager.Instance.SetBGM(null);
        }
        else AudioManager.Instance.SetBGM(audioClip.name);
    }
}
