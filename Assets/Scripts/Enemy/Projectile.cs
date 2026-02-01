using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    [SerializeField] private int damage = 10;
    [SerializeField] private bool isPlayerProjectile = true;

    [Header("Lifetime")]
    [SerializeField] private float lifetimeSeconds = 5f;
    [SerializeField] float spinDegreesPerSecond = 360f;   // set in prefab
    [SerializeField] Vector3 spinAxis = Vector3.forward;  // pick axis that looks right

    private void OnEnable()
    {
        // Ensure projectiles don't live forever if they miss everything.
        if (lifetimeSeconds > 0f)
        {
            CancelInvoke(nameof(Despawn));
            Invoke(nameof(Despawn), lifetimeSeconds);
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Despawn));
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }

    public void Initialize(Vector3 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
        spinDegreesPerSecond *= (Random.value < 0.5f) ? -1f : 1f;

    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        transform.Rotate(spinAxis, spinDegreesPerSecond * Time.deltaTime, Space.Self);

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