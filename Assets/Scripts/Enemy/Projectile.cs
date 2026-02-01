using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    [SerializeField] private int damage = 10;
    [SerializeField] private bool isPlayerProjectile = true;

    public void Initialize(Vector3 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        else if (!isPlayerProjectile && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<HealthController>(out var playerHealth))
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if(isPlayerProjectile && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<HealthController>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}