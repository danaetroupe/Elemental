using System.Collections;
using Unity.Netcode;
using Unity.Mathematics;
using UnityEngine;

public class EnemyController : RadialPower
{
    [Header("Movement Settings")]
    [SerializeField] private bool3 axis;
    [SerializeField] private float range = 5f; 
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private float timeBetweenFire = 2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool movingToTarget = true;
    private Coroutine fireRoutine;

    private void Start()
    {
        startPosition = transform.position;
        CalculateTargetPosition();

       StartCoroutine(BeginFiringWhenNetworkReady());
    }

    void Update()
    {
        // In multiplayer, the server is authoritative for enemy movement.
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening &&
            !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        MoveEnemy();
    }

    private IEnumerator BeginFiringWhenNetworkReady()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            fireRoutine = StartCoroutine(FireProjectiles());
            yield break;
        }

        yield return new WaitUntil(() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening);

        if (!NetworkManager.Singleton.IsServer)
        {
            yield break;
        }

        fireRoutine = StartCoroutine(FireProjectiles());
    }

    private void CalculateTargetPosition()
    {
        targetPosition = startPosition;

        if (axis.x) targetPosition.x += range;
        if (axis.y) targetPosition.y += range;
        if (axis.z) targetPosition.z += range;
    }

    private void MoveEnemy()
    {
        Vector3 destination = movingToTarget ? targetPosition : startPosition;
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            movingToTarget = !movingToTarget;
        }
    }

    private IEnumerator FireProjectiles()
    {
        while(true)
        {
            UsePower();
            yield return new WaitForSeconds(timeBetweenFire);
        }
    }
}