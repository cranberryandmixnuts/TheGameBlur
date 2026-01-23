using UnityEngine;
using UnityEngine.Playables;

public class PrologManager : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private PrologPlayerController prologPlayerController;

    private void Start()
    {
        playableDirector.Play();
    }

    public void OnExitPrologTimeline()
    {
        prologPlayerController.ActivePlayer();
        CinematicManager.Show<CinematicSubTutorial>();
    }
}
