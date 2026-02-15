using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemySpawnTarget> spawnTargets;
    [SerializeField] private EnemyScript enemyPrefab;

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
    }
}
