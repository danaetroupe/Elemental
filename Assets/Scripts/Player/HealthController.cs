using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class HealthController : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int starterHealth = 100;

    [Header("Events")]
    public UnityEvent<Vector3> OnDeath;
    public UnityEvent<int, int> OnHealthChanged;

    private int currentHealth;
    private readonly NetworkVariable<int> networkHealth = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private bool UseNetworkedHealth()
    {
        return NetworkManager.Singleton != null && IsSpawned;
    }

    private void Start()
    {
        if (!UseNetworkedHealth())
        {
            currentHealth = starterHealth;
            OnHealthChanged?.Invoke(currentHealth, starterHealth);
        }
    }

    public override void OnNetworkSpawn()
    {
        networkHealth.OnValueChanged += HandleNetworkHealthChanged;

        if (IsServer)
        {
            networkHealth.Value = starterHealth;
        }

        OnHealthChanged?.Invoke(networkHealth.Value, starterHealth);
    }

    public override void OnNetworkDespawn()
    {
        networkHealth.OnValueChanged -= HandleNetworkHealthChanged;
    }

    private void HandleNetworkHealthChanged(int previousValue, int newValue)
    {
        OnHealthChanged?.Invoke(newValue, starterHealth);
    }

    public void TakeDamage(int damage)
    {
        if (UseNetworkedHealth())
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            networkHealth.Value = Mathf.Max(networkHealth.Value - damage, 0);
            if (networkHealth.Value <= 0)
            {
                Die();
            }

            return;
        }

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        OnHealthChanged?.Invoke(currentHealth, starterHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (UseNetworkedHealth())
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            networkHealth.Value = Mathf.Min(networkHealth.Value + amount, starterHealth);
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, starterHealth);
        OnHealthChanged?.Invoke(currentHealth, starterHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke(gameObject.transform.position);

        if (TryGetComponent<NetworkObject>(out var no) && no.IsSpawned)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                no.Despawn(true);
            }
            return;
        }

        Destroy(gameObject);
    }

    public int GetCurrentHealth()
    {
        return UseNetworkedHealth() ? networkHealth.Value : currentHealth;
    }

    public int GetMaxHealth()
    {
        return starterHealth;
    }
}