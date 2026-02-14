using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachineManager : MonoBehaviour
{
    [SerializeField] private SlotMachineView slotMachineView;

    [Header("Probability")]
    [SerializeField] private float succesWeight;
    [SerializeField] private float failWeight;

    [Header("ResultSetting")]
    [SerializeField] private SlotMachineView.ImageType succesResult;
    [SerializeField] private SlotMachineView.ImageType failResult;

    [Header("Cooldown")]
    [SerializeField] private float cooldownDelay;

    private bool isCooldown = false;

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlaySlot()
    {
        if (isCooldown)
            return;

        isCooldown = true;
        StartCoroutine(CooldownDelay());

        var result = Random.Range(0, succesWeight + failWeight);
        List<SlotMachineView.ImageType> slotInfos = new();

        if (result <= succesWeight) // Succes
        {
            StartCoroutine(DelayedPlaySound("Jackpot", 1.2f));

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

    private IEnumerator CooldownDelay()
    {
        yield return new WaitForSeconds(cooldownDelay);
        isCooldown = false;
    }
}
