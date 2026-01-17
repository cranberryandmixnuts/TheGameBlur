using System;
using System.Collections;
using UnityEngine;

public class CinematicDialog : Cinematic
{
    private DialogData dialogData;

    private DialogEventReciever dialogEventReciever;
    private Action<DialogInfo> onFinished;
    private int dialogIndex = 0;

    public void BindDialog(string name)
    {
        dialogData = DialogRegistryManager.Instance.GetDialogData(name);

        Initialize();
    }

    private void Initialize()
    {
        dialogEventReciever = new GameObject("DialogEventReciever").AddComponent<DialogEventReciever>();
        ProcessDialog();
    }

    private void OnDialogFinished(DialogInfo dialogInfo)
    {
        ProcessDialog();
    }

    public override void Finish()
    {
        base.Finish();

        Destroy(dialogEventReciever.gameObject);
        Destroy(gameObject);
    }

    private void ProcessDialog()
    {
        if(dialogIndex >= dialogData.Dialogs.Count)
        {
            Finish();
            return;
        }

        UIManager.Instance.Show<DialogView>().Initialize(
            dialogData.Dialogs[dialogIndex], 
            dialogEventReciever, 
            OnDialogFinished
            );

        dialogIndex++;
    }
}
