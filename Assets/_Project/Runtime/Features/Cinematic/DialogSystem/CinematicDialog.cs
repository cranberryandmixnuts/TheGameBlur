using System;
using System.Collections;
using UnityEngine;

public class CinematicDialog : Cinematic
{
    private DialogData dialogData;

    private DialogEventReciever dialogEventReciever;
    private int dialogIndex = 0;

    [SerializeField] private SelectionView selectionViewPrefab;

    private SelectionView selectionView;
    private DialogView dialogView;

    public void BindDialog(string name)
    {
        dialogData = DialogRegistryManager.Instance.GetDialogData(name);

        Initialize();
    }

    public void BindDialog(DialogData dialogData)
    {
        this.dialogData = dialogData;

        Initialize();
    }

    private void Initialize()
    {
        dialogEventReciever = new GameObject(typeof(DialogEventReciever).Name).AddComponent<DialogEventReciever>();
        ProcessDialog();
    }

    private void OnDialogFinished(Dialog dialog)
    {
        if (dialog.IsSelectable)
        {
            selectionView = Instantiate(selectionViewPrefab);
            selectionView.Initialize(dialog.Selections, OnSelected);
        }
        else
        {
            Destroy(dialogView.gameObject);

            ProcessDialog();
        }
    }

    private void OnSelected(Selection selection)
    {
        dialogData = selection.DialogData;

        Destroy(selectionView.gameObject);
        Destroy(dialogView.gameObject);

        dialogIndex = 0;
        ProcessDialog();
    }

    private void ProcessDialog()
    {
        if(dialogIndex >= dialogData.Dialogs.Count)
        {
            Finish();
            return;
        }

        dialogView = UIManager.Show<DialogView>().Initialize(
            dialogData.Dialogs[dialogIndex], 
            dialogEventReciever, 
            OnDialogFinished
            );

        dialogIndex++;
    }

    public override void Finish()
    {
        base.Finish();

        Destroy(dialogEventReciever.gameObject);
        Destroy(gameObject);
    }

}
