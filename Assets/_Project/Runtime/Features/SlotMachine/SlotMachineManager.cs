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
            for(int index = 0; index < 3; index++)
            {
                slotInfos.Add(succesResult);
            }

            slotMachineView.PlaySlot(slotInfos);
        }
        else // Fail
        {
            for(int index = 0; index < 3; index++)
            {
                slotInfos.Add(failResult);
            }

            slotMachineView.PlaySlot(slotInfos);
            SceneController.Instance.LoadScene(SceneType.PrologScene, 3f);
        }
    }

    private IEnumerator CooldownDelay()
    {
        yield return new WaitForSeconds(cooldownDelay);
        isCooldown = false;
    }
}
