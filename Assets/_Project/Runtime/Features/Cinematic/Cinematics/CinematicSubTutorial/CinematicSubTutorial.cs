using System.Collections;
using UnityEngine;

public class CinematicSubTutorial : Cinematic
{
    private bool isMoved = false;

    private void Start()
    {
        StartCoroutine(PlayMoveTutorial());
    }

    private IEnumerator PlayMoveTutorial()
    {
        yield return new WaitForSeconds(7f);
        if (!isMoved)
        {
            var dialog = CinematicManager.Show<CinematicDialog>();
            dialog.BindDialog("SubTutorial_Cut3");
            dialog.OnFinished += OnFinishDialog;
        }
    }

    private void OnFinishDialog(Cinematic cinematic)
    {
        Finish();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            isMoved = true;
            Finish();
        }
    }
}
