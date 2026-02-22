using System.Collections.Generic;
using UnityEngine;

public class CinematicMainTutorial : Cinematic
{
    [SerializeField] private ClickGuideView clickGuideViewPrefab;
    private ClickGuideView clickGuideView;

    private Transform playerTransform;
    List<EnemyScript> enemies = new();
    private float ExplosionRange { get; } = 30f;
    private float TimeSlowRange { get; } = 3.5f;
    private float TimeSlowScale { get; } = 6f;
    private bool isTimeSlowed = false;
    private bool isTimeUnslowed = false;

    public override void Play()
    {
        base.Play();

        playerTransform = FindAnyObjectByType<Player>().transform;

        EnemyScript[] allEnemies = FindObjectsByType<EnemyScript>(FindObjectsSortMode.None);

        foreach (var enemy in allEnemies)
        {
            if (Vector3.Distance(playerTransform.position, enemy.transform.position) <= ExplosionRange)
            {
                enemies.Add(enemy);
            }
        }

        var enemyRigidbodies = new List<Rigidbody>();

        foreach (var enemyRigidbody in enemyRigidbodies)
        {
            var force = (enemyRigidbody.transform.position - playerTransform.position).ToVector2();
            enemyRigidbody.AddForce(force);
        }
    }

    private void Update()
    {
        if(!isTimeSlowed)
        {
            foreach (var enemy in enemies)
            {
                if (Vector3.Distance(playerTransform.position, enemy.transform.position) <= TimeSlowRange)
                {
                    isTimeSlowed = true;
                    clickGuideView = Instantiate(clickGuideViewPrefab);
                    clickGuideView.SetClickGuide(playerTransform, new Vector3(-100, 200, 0));
                }
            }
        }

        if(isTimeSlowed && Time.timeScale > 0 && !isTimeUnslowed)
        {
            if(Time.timeScale - Time.deltaTime * TimeSlowScale > 0.1f)
            {
                Time.timeScale -= Time.deltaTime * TimeSlowScale;
            }
            else
            {
                Time.timeScale = 0;
            }
        }
        
        if(Mathf.Approximately(Time.timeScale, 0) && InputManager.Instance.AttackDown)
        {
            isTimeUnslowed = true;
            Destroy(clickGuideView.gameObject);
        }

        if(isTimeUnslowed)
        {
            
            if (Time.timeScale + (Time.deltaTime + 0.001f) * TimeSlowScale < 1)
            {
                Time.timeScale += (Time.deltaTime + 0.001f) * TimeSlowScale;
            }
            else
            {
                Time.timeScale = 1;
                Finish();
            }
        }
    }

    public override void Finish()
    {
        base.Finish();
        Destroy(gameObject);
    }
}
