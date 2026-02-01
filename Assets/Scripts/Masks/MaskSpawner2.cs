using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


public class MaskSpawner2 : MonoBehaviour
{
    // the mask we want to spawn
    [Header("Mask Settings")]
    public GameObject maskToSpawn;

    // spawn settings
    [Header("Spawn Settings")]
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0);
    public bool spawnAtEnemyPosition = true;
    public Transform customSpawnLocation;

    // effects stuff
    [Header("Effects")]
    public GameObject spawnEffectPrefab;
    public float spawnDelay = 0.5f;

    // private variables
    private bool hasSpawned = false;
    private List<HealthController> enemyList = new List<HealthController>();
    private List<UnityAction<Vector3>> handlerList = new List<UnityAction<Vector3>>();
    private int enemyCount = 0;

    private bool CheckIfWeAreTheServer()
    {
        if (NetworkManager.Singleton == null)
        {
            return true;
        }
        
        if (NetworkManager.Singleton.IsListening == false)
        {
            return true;
        }
        
        if (NetworkManager.Singleton.IsServer == true)
        {
            return true;
        }
        
        return false;
    }

    void Start()
    {
        bool isServer = CheckIfWeAreTheServer();
        
        if (isServer == false)
        {
            this.enabled = false;
            return;
        }
        
        FindAndRegisterAllEnemies();
    }

    void OnDisable()
    {
        RemoveAllListeners();
    }

    private void FindAndRegisterAllEnemies()
    {
        enemyList.Clear();
        handlerList.Clear();
        enemyCount = 0;

        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        for (int i = 0; i < allEnemies.Length; i++)
        {
            GameObject enemyObject = allEnemies[i];
            
            HealthController health = enemyObject.GetComponent<HealthController>();
            
            if (health != null)
            {
                RegisterEnemy(health);
            }
        }
        
    }

    public void RegisterEnemy(HealthController enemy)
    {
        if (enemy == null)
        {
            return;
        }
        
        bool alreadyHaveIt = false;
        for (int i = 0; i < enemyList.Count; i++)
        {
            if (enemyList[i] == enemy)
            {
                alreadyHaveIt = true;
                break;
            }
        }
        
        if (alreadyHaveIt == true)
        {
            return;
        }


        HealthController thisEnemy = enemy;
        UnityAction<Vector3> deathHandler = delegate(Vector3 pos) 
        {
            HandleEnemyDeath(thisEnemy, pos);
        };
        
        enemyList.Add(enemy);
        handlerList.Add(deathHandler);
        
        enemy.OnDeath.AddListener(deathHandler);
        
        enemyCount = enemyCount + 1;
    }

    public void UnregisterEnemy(HealthController enemy)
    {
        if (enemy == null)
        {
            return;
        }
        
        int indexToRemove = -1;
        for (int i = 0; i < enemyList.Count; i++)
        {
            if (enemyList[i] == enemy)
            {
                indexToRemove = i;
                break;
            }
        }
        
        if (indexToRemove != -1)
        {
            UnityAction<Vector3> handler = handlerList[indexToRemove];
            
            enemy.OnDeath.RemoveListener(handler);
            
            enemyList.RemoveAt(indexToRemove);
            handlerList.RemoveAt(indexToRemove);
            
            enemyCount = enemyCount - 1;
            if (enemyCount < 0)
            {
                enemyCount = 0;
            }
        }
    }

    private void RemoveAllListeners()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            HealthController enemy = enemyList[i];
            UnityAction<Vector3> handler = handlerList[i];
            
            if (enemy != null)
            {
                enemy.OnDeath.RemoveListener(handler);
            }
        }
        
        enemyList.Clear();
        handlerList.Clear();
    }

    private void HandleEnemyDeath(HealthController enemy, Vector3 deathPosition)
    {
        if (hasSpawned == true)
        {
            return;
        }

        UnregisterEnemy(enemy);

        if (enemyCount <= 0)
        {
            OnLastEnemyDeath(deathPosition);
        }
    }

    public void OnLastEnemyDeath(Vector3 enemyPosition)
    {
        if (hasSpawned == true)
        {
            Debug.LogWarning("Mask has already been spawned!");
            return;
        }

        if (maskToSpawn == null)
        {
            Debug.LogError("MaskToSpawn is not assigned!");
            return;
        }

        Vector3 whereToSpawn = GetSpawnPosition(enemyPosition);

        if (spawnDelay > 0)
        {
            StartCoroutine(WaitThenSpawnMask(whereToSpawn));
        }
        else
        {
            DoSpawnMask(whereToSpawn);
        }
    }

    private IEnumerator WaitThenSpawnMask(Vector3 pos)
    {
        yield return new WaitForSeconds(spawnDelay);
        
        DoSpawnMask(pos);
    }

    private void DoSpawnMask(Vector3 position)
    {
        if (hasSpawned == true)
        {
            return;
        }

        Quaternion rot = Quaternion.Euler(0, 0, 0);
        
        GameObject newMask = Instantiate(maskToSpawn, position, rot);

        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsListening == true)
            {
                if (NetworkManager.Singleton.IsServer == true)
                {
                    NetworkObject netObj = newMask.GetComponent<NetworkObject>();
                    
                    if (netObj != null)
                    {
                        if (netObj.IsSpawned == false)
                        {
                            netObj.Spawn();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[MaskSpawnerServerAuthoritative] MaskToSpawn has no NetworkObject; it will only exist on the server.");
                    }
                }
            }
        }

        DoSpawnEffect(position, rot);
        
        hasSpawned = true;
        
        Debug.Log("Mask spawned at position: " + position.ToString());
    }

    private void DoSpawnEffect(Vector3 position, Quaternion rotation)
    {
        if (spawnEffectPrefab == null)
        {
            return;
        }

        GameObject effectObj = Instantiate(spawnEffectPrefab, position, rotation);

        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsListening == true)
            {
                if (NetworkManager.Singleton.IsServer == true)
                {
                    NetworkObject netObj = effectObj.GetComponent<NetworkObject>();
                    
                    if (netObj != null)
                    {
                        if (netObj.IsSpawned == false)
                        {
                            netObj.Spawn();
                        }
                    }
                }
            }
        }

        Destroy(effectObj, 2.0f);
    }

    private Vector3 GetSpawnPosition(Vector3 enemyPos)
    {
        Vector3 result;

        if (customSpawnLocation != null)
        {
            result = customSpawnLocation.position;
        }
        else
        {
            if (spawnAtEnemyPosition == true)
            {
                result = enemyPos + spawnOffset;
            }
            else
            {
                result = transform.position + spawnOffset;
            }
        }

        return result;
    }
}
