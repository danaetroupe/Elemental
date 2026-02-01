using System.Collections.Generic;
using UnityEngine;

public class MaskSpawner : MonoBehaviour
{
    [Header("Mask Settings")]
    public GameObject maskToSpawn;

    [Header("Spawn Settings")]
    public Vector3 spawnOffset = Vector3.up * 0.5f;
    public bool spawnAtEnemyPosition = true;
    public Transform customSpawnLocation;

    [Header("Effects")]
    public GameObject spawnEffectPrefab;
    public float spawnDelay = 0.5f;

    private bool hasSpawned = false;

    void Start()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            HealthController healthController = enemy.GetComponent<HealthController>();
            if (healthController != null)
            {
                healthController.OnDeath.AddListener(OnEnemyDeath);
            }
        }
    }

    public void OnEnemyDeath(Vector3 enemyPosition)
    {
        if (!GameObject.FindGameObjectWithTag("Enemy"))
        {
            OnLastEnemyDeath(enemyPosition);
        }
    }
    public void OnLastEnemyDeath(Vector3 enemyPosition)
    {
        if (hasSpawned)
        {
            Debug.LogWarning("Mask has already been spawned!");
            return;
        }

        if (maskToSpawn == null)
        {
            Debug.LogError("MaskToSpawn is not assigned!");
            return;
        }

        Vector3 spawnPosition = DetermineSpawnPosition(enemyPosition);

        if (spawnDelay > 0)
        {
            Invoke(nameof(SpawnMaskDelayed), spawnDelay);
            lastSpawnPosition = spawnPosition;
        }
        else
        {
            SpawnMask(spawnPosition);
        }
    }

    private Vector3 lastSpawnPosition;

    private void SpawnMaskDelayed()
    {
        SpawnMask(lastSpawnPosition);
    }

    private void SpawnMask(Vector3 position)
    {
        // Instantiate the mask prefab
        GameObject spawnedMask = Instantiate(maskToSpawn, position, Quaternion.identity);

        // Spawn visual effect if assigned
        if (spawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(spawnEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f); // Auto-destroy effect after 2 seconds
        }

        hasSpawned = true;
        Debug.Log($"Mask spawned at position: {position}");
    }

    private Vector3 DetermineSpawnPosition(Vector3 enemyPosition)
    {
        Vector3 spawnPosition;

        if (customSpawnLocation != null)
        {
            // Use custom spawn location if specified
            spawnPosition = customSpawnLocation.position;
        }
        else if (spawnAtEnemyPosition)
        {
            // Spawn at enemy position with offset
            spawnPosition = enemyPosition + spawnOffset;
        }
        else
        {
            // Spawn at this spawner's position
            spawnPosition = transform.position + spawnOffset;
        }

        return spawnPosition;
    }
}