using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class PrologManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector prologTimelime;
    [SerializeField] private PlayableDirector goldBugTimeline;
    [SerializeField] private PrologPlayerController prologPlayerController;
    [SerializeField] private GameObject devil;

    private void Start()
    {
        prologTimelime.Play();
    }

    public void StartGoldBug()
    {
        goldBugTimeline.Play();
    }

    public void OnExitPrologTimeline()
    {
        
    }

    public void OnExitGoldBugPrologTimeline()
    {
        devil.SetActive(true);
        StartCoroutine(DelayStartDialog());
    }

    private IEnumerator DelayStartDialog()
    {
        yield return new WaitForSeconds(1f);
        var cinematicGoldBugDialog = CinematicManager.Show<CinematicGoldBugDialog>();
        cinematicGoldBugDialog.OnFinished += OnEndCinematicPrologDialog;
        cinematicGoldBugDialog.StartDialog();
    }

    public void OnEndCinematicPrologDialog(Cinematic cinematic)
    {

    }

    public void Play_DialogSubtutorialCut2()
    {
        var dialog = CinematicManager.Show<CinematicDialog>();
        dialog.BindDialog("SubTutorial_Cut2");
        dialog.OnFinished += OnEnd_DialogSubTutorialCut2;
    }

    public void OnEnd_DialogSubTutorialCut2(Cinematic cinematic)
    {
        prologPlayerController.ActivePlayer();
        CinematicManager.Show<CinematicSubTutorial>();
    }
}
