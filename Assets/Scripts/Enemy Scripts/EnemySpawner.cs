using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int maxAliveEnemies = 3;
    [SerializeField] private float respawnDelay = 8f;
    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private bool spawnOnStart = true;

    [Header("Optional")]
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private LayerMask groundMask = ~0;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private bool isRespawning;

    private void Start()
    {
        if (spawnOnStart)
        {
            for (int i = 0; i < maxAliveEnemies; i++)
            {
                SpawnEnemy();
            }
        }
    }

    private void Update()
    {
        CleanupDeadReferences();

        if (!isRespawning && aliveEnemies.Count < maxAliveEnemies)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        while (aliveEnemies.Count < maxAliveEnemies)
        {
            yield return new WaitForSeconds(respawnDelay);
            CleanupDeadReferences();

            if (aliveEnemies.Count < maxAliveEnemies)
            {
                SpawnEnemy();
            }
        }

        isRespawning = false;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"EnemySpawner on {name} has no enemy prefab assigned.");
            return;
        }

        Vector3 center = spawnCenter != null ? spawnCenter.position : transform.position;
        Vector3 spawnPos = GetSpawnPosition(center);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform);
        aliveEnemies.Add(enemy);
    }

    private Vector3 GetSpawnPosition(Vector3 center)
    {
        for (int i = 0; i < 12; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = center + new Vector3(randomCircle.x, 5f, randomCircle.y);

            if (Physics.Raycast(candidate, Vector3.down, out RaycastHit hit, 20f, groundMask))
            {
                Vector3 groundedPoint = hit.point;

                if (NavMesh.SamplePosition(groundedPoint, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                {
                    return navHit.position;
                }

                return groundedPoint;
            }
        }

        return center;
    }

    private void CleanupDeadReferences()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = spawnCenter != null ? spawnCenter.position : transform.position;
        Gizmos.DrawWireSphere(center, spawnRadius);
    }
}