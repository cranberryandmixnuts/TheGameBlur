using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SlotMachineManager : MonoBehaviour
{
    [SerializeField] private SlotMachineView slotMachineView;

    [Header("Probability")]
    [SerializeField] private float succesWeight;
    [SerializeField] private float failWeight;
    [SerializeField] private int maxSuccesStack;

    [Header("ResultSetting")]
    [SerializeField] private SlotMachineView.ImageType succesResult;
    [SerializeField] private SlotMachineView.ImageType failResult;

    [Header("Effects")]
    [SerializeField] private VisualEffect blinkEffect;

    private bool isCooldown = false;

    private int succesStack = 0;

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlaySlot()
    {
        if (isCooldown)
            return;

        blinkEffect.Stop();

        isCooldown = true;
        var result = Random.Range(0, succesWeight + failWeight);
        List<SlotMachineView.ImageType> slotInfos = new();

        AudioManager.Instance.PlaySFX("SlotMachineReel");

        if (result <= succesWeight && succesStack < maxSuccesStack) // Succes
        {
            succesStack++;

            StartCoroutine(DelayedPlaySound("Jackpot", 1.2f));
            StartCoroutine(DelayedSuccesComment(2.5f));

            for(int index = 0; index < 3; index++)
            {
                slotInfos.Add(succesResult);
            }

            slotMachineView.PlaySlot(slotInfos);
        }
        else // Fail
        {
            StartCoroutine(DelayedPlaySound("Fail", 1f));

            for (int index = 0; index < 3; index++)
            {
                slotInfos.Add(failResult);
            }

            slotMachineView.PlaySlot(slotInfos);
            SceneController.Instance.LoadScene(SceneType.PrologScene, 3f);
        }
    }

    private IEnumerator DelayedPlaySound(string soundName, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance.PlaySFX(soundName);
    }

    private IEnumerator DelayedSuccesComment(float delay)
    {
        yield return new WaitForSeconds(delay);
        var cinematic = CinematicManager.Show<CinematicSuccesSlotMachine>();
        cinematic.Play();
        cinematic.OnFinished += OnSuccesCommentFinisihed;
    }

    private void OnSuccesCommentFinisihed(Cinematic obj)
    {
        blinkEffect.Play();
        isCooldown = false;
    }
}
