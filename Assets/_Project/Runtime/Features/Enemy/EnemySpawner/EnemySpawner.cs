using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemySpawnTarget> spawnTargets;
    [SerializeField] private EnemyScript enemyPrefab;
    [SerializeField] private bool IsNotTutorial = false;

    private List<EnemyScript> spawnScripts = new();

    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        foreach (EnemySpawnTarget target in spawnTargets)
        {
            var enemy = Instantiate(enemyPrefab);
            enemy.transform.position = 
                new Vector3(target.transform.position.x, target.transform.position.y, 0);
            
            spawnScripts.Add(enemy);
        }

        if (IsNotTutorial)
        {
           foreach(EnemyScript script in spawnScripts) { 
            
            script.ActivateEnemy();
            
           }
        }
    }
}
