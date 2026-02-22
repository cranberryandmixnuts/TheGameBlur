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
    private Dialog dialog;
    public event Action<Dialog> OnFinished;

    private Coroutine updateDialogCoroutine;
    private bool isStopped = false;

    public DialogView Initialize(Dialog dialog, DialogEventReciever dialogEventReciever, Action<Dialog> onFinished = null)
    {
        this.dialog = dialog;
        nameText.text = dialog.Name;
        lineText.text = dialog.Line;
        this.dialogEventReciever = dialogEventReciever;
        if(onFinished != null) OnFinished += onFinished;

        StartDialog();

        return this;
    }

    private void Update()
    {
        if(InputManager.Instance.AttackDown)
            SkipDialog();
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
        if (isStopped)
            return;

        StopDialog();
        lineText.maxVisibleCharacters = lineText.text.Length;
    }

    public void StopDialog()
    {
        if (updateDialogCoroutine != null)
        {
            StopCoroutine(updateDialogCoroutine);
        }

        isStopped = true;
        StartCoroutine(EndDialog());
    }

    private IEnumerator EndDialog()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => InputManager.Instance.AttackDown);

        OnFinished?.Invoke(dialog);
    }
}
