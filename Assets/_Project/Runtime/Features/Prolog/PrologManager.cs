using UnityEngine;
using UnityEngine.Playables;

public class PrologManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector prologTimelime;
    [SerializeField] private PlayableDirector goldBugTimeline;
    [SerializeField] private PrologPlayerController prologPlayerController;

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
