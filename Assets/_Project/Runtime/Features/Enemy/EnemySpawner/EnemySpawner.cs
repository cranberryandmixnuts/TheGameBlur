using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemySpawnTarget> spawnTargets;
    [SerializeField] private EnemyScript enemyPrefab;
    [SerializeField] private bool isActivate = false;

    private List<EnemyScript> spawnScripts = new();

    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        spawnScripts.Clear();

        foreach (EnemySpawnTarget target in spawnTargets)
        {
            if (target == null) continue;

            EnemyScript enemy = Instantiate(
                enemyPrefab,
                new Vector3(target.transform.position.x, target.transform.position.y, 0f),
                Quaternion.identity
            );

            spawnScripts.Add(enemy);
        }

        if (isActivate)
        {
            StartCoroutine(ActivateNextFrame());
        }
    }

    private IEnumerator ActivateNextFrame()
    {
        yield return null; // 다음 프레임까지 대기

        foreach (EnemyScript enemy in spawnScripts)
        {
            if (enemy != null)
            {
                enemy.ActivateEnemy();
            }
        }
    }
}