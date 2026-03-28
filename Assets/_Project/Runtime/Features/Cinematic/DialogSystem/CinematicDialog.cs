using System;
using System.Collections;
using UnityEngine;

public class CinematicDialog : Cinematic
{
    private static CinematicDialog instance { get; set; } = null;

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

        if(instance != null )
        {
            Destroy(instance.gameObject);
        }
        instance = this;
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

        if(dialogEventReciever != null) Destroy(dialogEventReciever.gameObject);
        if (dialogView != null) Destroy(dialogView.gameObject);
        if (selectionView != null) Destroy(selectionView.gameObject);
        Destroy(gameObject);
    }


    private void OnDisable()
    {
        if (dialogEventReciever != null) Destroy(dialogEventReciever.gameObject);
        if (selectionView != null) Destroy(selectionView.gameObject);
        if (dialogView != null) Destroy(dialogView.gameObject);
    }
}
