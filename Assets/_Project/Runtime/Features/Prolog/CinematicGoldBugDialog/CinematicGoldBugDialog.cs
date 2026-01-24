using System.Collections;
using UnityEngine;

public class CinematicGoldBugDialog : Cinematic
{
    [SerializeField] private DialogData goldBugDialog;

    private DialogEventReciever dialogEventReciever;
    private int dialogIndex = 0;

    [SerializeField] private GoldBugSelectionView selectionViewPrefab;

    private GoldBugSelectionView selectionView;
    private DialogView dialogView;

    public void StartDialog()
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
            StartCoroutine(ForceProcessDialog());
        }
        else
        {
            Destroy(dialogView.gameObject);

            ProcessDialog();
        }
    }

    private IEnumerator ForceProcessDialog()
    {
        yield return new WaitForSeconds(1f);

        Destroy(dialogView.gameObject);
        Destroy(selectionView.gameObject);

        ProcessDialog();
    }

    private void OnSelected(Selection selection)
    {

    }

    private void ProcessDialog()
    {
        if (dialogIndex >= goldBugDialog.Dialogs.Count)
        {
            Finish();
            return;
        }

        dialogView = UIManager.Show<DialogView>().Initialize(
            goldBugDialog.Dialogs[dialogIndex],
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
