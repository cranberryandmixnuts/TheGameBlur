using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.VFX;

public class CinematicSuccesSlotMachine : Cinematic
{
    [SerializeField] private List<DialogData> dialogDatas;
    [SerializeField] private VisualEffect visualEffectPrefab;

    private VisualEffect visualEffect = null;

    public override void Play()
    {
        var randomIndex = Random.Range(0, dialogDatas.Count);

        var cinematicDialog = CinematicManager.Show<CinematicDialog>();
        cinematicDialog.BindDialog(dialogDatas[randomIndex]);
        cinematicDialog.OnFinished += Finish;

        visualEffect = Instantiate(visualEffectPrefab);
        visualEffect.Play();

        StartCoroutine(StopVisualEffect());
    }

    private IEnumerator StopVisualEffect()
    {
        yield return new WaitForSeconds(1f);

        visualEffect.Stop();
    }

    private void Finish(Cinematic obj)
    {
        Finish();
    }
}
