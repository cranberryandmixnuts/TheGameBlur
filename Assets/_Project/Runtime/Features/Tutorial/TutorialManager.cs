using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private Animator leech;
    [SerializeField] private CameraController cameraController;

    private float explosionRange { get; } = 30f;
    private Transform playerTransform;
    List<EnemyScript> enemies = new List<EnemyScript>();

    private void Start()
    {
        playerTransform = FindAnyObjectByType<Player>().transform;

        EnemyScript[] allEnemies = FindObjectsByType<EnemyScript>(FindObjectsSortMode.None);

        foreach (var enemy in allEnemies)
        {
            if (Vector3.Distance(playerTransform.position, enemy.transform.position) <= explosionRange)
            {
                enemies.Add(enemy);
            }
        }

        foreach (var enemy in enemies)
        {
            enemy.DeactivateEnemy();
            Debug.Log("Deactive ½ÇÇà");
        }

        Player.Instance.Stats.PlayerSetActive(false);
    }

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
        StartCoroutine(StartMainTutorial3());
    }

    private IEnumerator StartMainTutorial3()
    {
        leech.gameObject.SetActive(true);
        leech.Play("LeechFall");

        yield return new WaitForSeconds(1.5f);

        var cinematicDialog = CinematicManager.Show<CinematicDialog>();
        cinematicDialog.BindDialog("MainTutorial4");
        cinematicDialog.OnFinished += OnEndMainTutorial3;
    }
    
    private void OnEndMainTutorial3(Cinematic cinematic)
    {
        var cinematicDialog = CinematicManager.Show<CinematicDialog>();
        cinematicDialog.BindDialog("MainTutorial3");

        foreach (var enemy in enemies)
        {
            enemy.MoveToTarget(playerTransform, 7f);
        }

        StartCoroutine(StartFightByDelay());
    }

    private IEnumerator StartFightByDelay()
    {
        cameraController.GetComponent<Animator>().Play("EnemyRunning", 0, 0f);
        Destroy(leech.gameObject);

        yield return new WaitForSeconds(2f);

        Destroy(cameraController.GetComponent<Animator>());

        EnemyScript[] allEnemies = FindObjectsByType<EnemyScript>(FindObjectsSortMode.None);

        foreach (var enemy in allEnemies)
        {
            enemy.ActivateEnemy();
        }

        Player.Instance.Stats.PlayerSetActive(true);
        cameraController.Active();
        playerUI.gameObject.SetActive(true);
        CinematicManager.Show<CinematicMainTutorial>().Play();
    }
}
