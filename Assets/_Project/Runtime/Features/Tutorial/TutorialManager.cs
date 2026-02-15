using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private Animator leech;
    [SerializeField] private CameraController cameraController;

    public void OnEndCinemtaic()
    {
        var cinematicDialog = CinematicManager.Show<CinematicDialog>();
        cinematicDialog.BindDialog("MainTutorial1");
        cinematicDialog.OnFinished += OnEndMainTutorial1;
    }

    private void OnEndMainTutorial1(Cinematic cinematic)
    {
        var cinematicDialog = CinematicManager.Show<CinematicDialog>();
        cinematicDialog.BindDialog("MainTutorial2");
        cinematicDialog.OnFinished += OnEndMainTutorial2;
    }

    private void OnEndMainTutorial2(Cinematic cinematic)
    {
        var cinematicDialog = CinematicManager.Show<CinematicDialog>();
        cinematicDialog.BindDialog("MainTutorial3");
        cinematicDialog.OnFinished += OnEndMainTutorial3;
    }
    
    private void OnEndMainTutorial3(Cinematic cinematic)
    {
        StartCoroutine(StartMainTutorial4());
    }

    private IEnumerator StartMainTutorial4()
    {
        leech.gameObject.SetActive(true);
        leech.Play("LeechFall");

        yield return new WaitForSeconds(1.5f);

        var cinematicDialog = CinematicManager.Show<CinematicDialog>();
        cinematicDialog.BindDialog("MainTutorial4");
        cinematicDialog.OnFinished += OnEndMainTutorial4;
    }


    private void OnEndMainTutorial4(Cinematic cinematic)
    {
        Destroy(leech.gameObject);
        StartFight();
    }

    private void StartFight()
    {
        cameraController.Active();
        playerUI.gameObject.SetActive(true);
        CinematicManager.Show<CinematicMainTutorial>().Play();
    }
}
