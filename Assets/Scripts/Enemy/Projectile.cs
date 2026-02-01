using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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

    private bool IsNetworkSpawned()
    {
        return TryGetComponent<NetworkObject>(out var no) && no.IsSpawned;
    }

    private bool IsServerAuthoritative()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    }

    private void OnEnable()
    {
        // For network-spawned projectiles, only the server controls lifetime.
        if (IsNetworkSpawned() && !IsServerAuthoritative()) return;

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
        if (TryGetComponent<NetworkObject>(out var no) && no.IsSpawned)
        {
            if (IsServerAuthoritative())
            {
                no.Despawn(true);
            }
            return;
        }

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
        // If this projectile is network-spawned, only the server simulates it.
        if (IsNetworkSpawned() && !IsServerAuthoritative()) return;

        transform.position += direction * speed * Time.deltaTime;
        transform.Rotate(spinAxis, spinDegreesPerSecond * Time.deltaTime, Space.Self);

        // Despawn if it falls below the play area.
        if (transform.position.y <= 0f)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If this projectile is network-spawned, only the server applies hits/despawns.
        if (IsNetworkSpawned() && !IsServerAuthoritative()) return;

        if (other.CompareTag("Wall"))
        {
            Despawn();
        }
        else if (!isPlayerProjectile && other.CompareTag("Player"))
        {
            if (other.TryGetComponent<HealthController>(out var playerHealth))
            {
                playerHealth.TakeDamage(damage);
            }
            Despawn();
        }
        else if(isPlayerProjectile && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<HealthController>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
            }
            Despawn();
        }
    }
}