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
        if(!isMoved)
        {
            CinematicManager.Show<CinematicDialog>().BindDialog("SubTutorial_Cut3");
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            isMoved = true;
        }
    }
}
