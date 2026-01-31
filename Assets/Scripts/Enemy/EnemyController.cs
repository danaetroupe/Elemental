using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool3 axis;
    [SerializeField] private float range = 5f; 
    [SerializeField] private float moveSpeed = 2f; 

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; 
    [SerializeField] private int numBullets = 8; 
    [SerializeField] private float timeBetweenFire = 2f;
    [SerializeField] private float bulletSpeed = 5f; 

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
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenFire);

            float angleStep = 360f / numBullets;

            for (int i = 0; i < numBullets; i++)
            {
                float angle = i * angleStep;

                float angleInRadians = angle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angleInRadians), 0, Mathf.Sin(angleInRadians));

                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

                // Add and configure projectile component
                Projectile projScript = projectile.GetComponent<Projectile>();
                if (projScript == null)
                {
                    projScript = projectile.AddComponent<Projectile>();
                }

                projScript.Initialize(direction, bulletSpeed);
            }
        }
    }
}