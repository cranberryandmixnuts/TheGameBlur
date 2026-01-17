using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogView : UIView
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private float speed;

    private DialogEventReciever dialogEventReciever;
    public event Action<DialogInfo> OnFinished;

    private Coroutine updateDialogCoroutine;

    public DialogView Initialize(Dialog dialog, DialogEventReciever dialogEventReciever, Action<DialogInfo> onFinished = null)
    {
        nameText.text = dialog.Name;
        lineText.text = dialog.Line;
        this.dialogEventReciever = dialogEventReciever;
        if(onFinished != null) OnFinished += onFinished;

        StartDialog();

        return this;
    }

    private void StartDialog()
    {
        updateDialogCoroutine = StartCoroutine(UpdateDialog());
    }

    private IEnumerator UpdateDialog()
    {
        int lineIndex = 0;
        lineText.maxVisibleCharacters = lineIndex;

        while (lineIndex != lineText.text.Length)
        {
            yield return new WaitForSeconds(speed);

            lineIndex++;
            lineText.maxVisibleCharacters = lineIndex;
        }

        StopDialog();
    }

    public void SkipDialog()
    {
        StopDialog();
        lineText.maxVisibleCharacters = lineText.text.Length;
    }

    public void StopDialog()
    {
        if (updateDialogCoroutine != null)
        {
            StopCoroutine(updateDialogCoroutine);
        }

        StartCoroutine(EndDialog());
    }

    private IEnumerator EndDialog()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        OnFinished?.Invoke(new());
        Destroy(gameObject);
    }
}
