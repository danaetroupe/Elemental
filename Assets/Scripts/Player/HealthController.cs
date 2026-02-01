using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class HealthController : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int starterHealth = 100;

    [Header("Events")]
    public UnityEvent<Vector3> OnDeath;
    public UnityEvent<int, int> OnHealthChanged;

    private int currentHealth;

    void Start()
    {
        currentHealth = starterHealth;
        OnHealthChanged?.Invoke(currentHealth, starterHealth);
    }

    public void TakeDamage(int damage)
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening &&
            !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, starterHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, starterHealth);

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
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return starterHealth;
    }
}