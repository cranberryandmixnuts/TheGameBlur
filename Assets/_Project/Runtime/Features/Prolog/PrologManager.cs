using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class PrologManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector prologTimelime;
    [SerializeField] private PlayableDirector goldBugTimeline;
    [SerializeField] private PlayableDirector fallTimeline;
    [SerializeField] private PrologPlayerController prologPlayerController;
    [SerializeField] private CameraShaker cameraShaker;
    [SerializeField] private GameObject devil;

    private CinematicDialog cinematicDialog;

    private void Start()
    {
        AudioManager.Instance.SetBGM("RainCity");
    }

    public void OnEndSceneFade()
    {
        prologTimelime.Play();
    }

    public void OnEndProlog()
    {
        UIManager.Show<DarkSolidView>();
        SceneController.Instance.LoadScene(SceneType.FallScene);
    }

    public void StartGoldBug()
    {
        var dialog = CinematicManager.Show<CinematicDialog>();

        dialog.BindDialog("DiceKick");
        dialog.OnFinished += OnEndDiceKickDialog;
    }

    private void OnEndDiceKickDialog(Cinematic obj)
    {
        goldBugTimeline.Play();
    }

    public void OnExitPrologTimeline()
    {
        prologPlayerController.ActivePlayer();
        CinematicManager.Show<CinematicSubTutorial>();
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
        cameraShaker.Shake();
        StartCoroutine(PlayFallTimeline());
    }

    private IEnumerator PlayFallTimeline()
    {
        yield return new WaitForSeconds(1f);
        fallTimeline.Play();
    }

    public void PlayDialog(DialogData dialogData)
    {
        cinematicDialog = CinematicManager.Show<CinematicDialog>();
        cinematicDialog.BindDialog(dialogData);
    }

    public void DestroyDialog()
    {
        if(cinematicDialog != null)
        {
            cinematicDialog.Finish();
        }
    }

    public void Play_DialogSubtutorialCut2()
    {
        var dialog = CinematicManager.Show<CinematicDialog>();
        dialog.BindDialog("SubTutorial_Cut2");
        dialog.OnFinished += OnEnd_DialogSubTutorialCut2;
    }

    public void OnEnd_DialogSubTutorialCut2(Cinematic cinematic)
    {
        
    }
}
