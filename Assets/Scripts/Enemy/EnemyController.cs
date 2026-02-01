using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

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

    void Start()
    {
        startPosition = transform.position;
        CalculateTargetPosition();
        StartCoroutine(FireProjectiles());
    }

    void Update()
    {
        MoveEnemy();
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