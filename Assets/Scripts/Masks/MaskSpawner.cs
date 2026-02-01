using System.Collections.Generic;
using UnityEngine;

public class MaskSpawner : MonoBehaviour
{
    [Header("Enemy References")]
    [SerializeField] private List<GameObject> enemiesToTrack = new List<GameObject>();

    [Header("Mask Settings")]
    public GameObject maskToSpawn;

    [Header("Spawn Settings")]
    public Vector3 spawnOffset = Vector3.up * 0.5f;
    public bool spawnAtEnemyPosition = true;
    public Transform customSpawnLocation;
    public float spawnDelay = 0.5f;

    private bool hasSpawned = false;
    private int remainingEnemies;

    void Start()
    {
        // Remove any null references from the list
        enemiesToTrack.RemoveAll(enemy => enemy == null);

        remainingEnemies = enemiesToTrack.Count;

        if (remainingEnemies == 0)
        {
            Debug.LogWarning("No enemies assigned to MaskSpawner!");
            return;
        }

        // Subscribe to death events for each enemy in the list
        foreach (GameObject enemy in enemiesToTrack)
        {
            HealthController healthController = enemy.GetComponent<HealthController>();
            if (healthController != null)
            {
                healthController.OnDeath.AddListener(OnEnemyDeath);
            }
            else
            {
                Debug.LogWarning($"Enemy '{enemy.name}' does not have a HealthController component!");
            }
        }
    }

    public void OnEnemyDeath(Vector3 enemyPosition)
    {
        remainingEnemies--;

        if (remainingEnemies <= 0)
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
        GameObject spawnedMask = Instantiate(maskToSpawn, position, Quaternion.Euler(90, 0, 0));

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