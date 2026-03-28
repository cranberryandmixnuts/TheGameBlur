using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CinematicSuccesSlotMachine : Cinematic
{
    [SerializeField] private List<DialogData> dialogDatas;

    public override void Play()
    {
        var randomIndex = Random.Range(0, dialogDatas.Count);

        var cinematicDialog = CinematicManager.Show<CinematicDialog>();
        cinematicDialog.BindDialog(dialogDatas[randomIndex]);
        cinematicDialog.OnFinished += Finish;
    }

    private void Finish(Cinematic obj)
    {
        Finish();
    }
}
