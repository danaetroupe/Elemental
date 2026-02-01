using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class HealthController : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int starterHealth = 100;

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float deathSoundVolume = 1f;
    [SerializeField] private AudioClip respawnSound;
    [SerializeField] private float respawnSoundVolume = 1f;

    [Header("Events")]
    public UnityEvent<Vector3> OnDeath;
    public UnityEvent<int, int> OnHealthChanged;

    private int currentHealth;
    private Vector3 spawnPosition;
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
            spawnPosition = transform.position;
            OnHealthChanged?.Invoke(currentHealth, starterHealth);
        }
    }

    public override void OnNetworkSpawn()
    {
        networkHealth.OnValueChanged += HandleNetworkHealthChanged;
        spawnPosition = transform.position;

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

        // Player respawn path
        if (TryGetComponent<PlayerControls>(out _))
        {
            Respawn();
            return;
        }

        // Non-player death (enemies, etc.) - play death sound then despawn
        PlayDeathSound();

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

    private void PlayDeathSound()
    {
        if (deathSound == null) return;

        Vector3 deathPosition = transform.position;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && IsSpawned)
        {
            // Broadcast to all clients before despawn
            PlayDeathSoundClientRpc(deathPosition);
        }
        else if (NetworkManager.Singleton == null)
        {
            // Offline mode
            AudioSource.PlayClipAtPoint(deathSound, deathPosition, deathSoundVolume);
        }
    }

    [ClientRpc]
    private void PlayDeathSoundClientRpc(Vector3 position)
    {
        // Play at the death position (object may be destroyed, so we use the passed position)
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, position, deathSoundVolume);
        }
    }

    private void Respawn()
    {
        if (UseNetworkedHealth())
        {
            if (!NetworkManager.Singleton.IsServer) return;

            // Play respawn sound for all clients
            PlayRespawnSound();

            // Reset health (server-authoritative via NetworkVariable)
            networkHealth.Value = starterHealth;
            
            // Reset position - use ClientRpc because NetworkTransform has Owner authority
            // The owning client must set their own position
            TeleportToSpawnClientRpc(spawnPosition);
        }
        else
        {
            // Offline mode
            PlayRespawnSoundLocal();
            currentHealth = starterHealth;
            transform.position = spawnPosition;
            OnHealthChanged?.Invoke(currentHealth, starterHealth);
        }
    }

    private void PlayRespawnSound()
    {
        if (respawnSound == null) return;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && IsSpawned)
        {
            PlayRespawnSoundClientRpc();
        }
    }

    private void PlayRespawnSoundLocal()
    {
        if (respawnSound != null)
        {
            AudioSource.PlayClipAtPoint(respawnSound, transform.position, respawnSoundVolume);
        }
    }

    [ClientRpc]
    private void PlayRespawnSoundClientRpc()
    {
        if (respawnSound != null)
        {
            AudioSource.PlayClipAtPoint(respawnSound, transform.position, respawnSoundVolume);
        }
    }

    [ClientRpc]
    private void TeleportToSpawnClientRpc(Vector3 position)
    {
        // Each client sets their local transform position
        // NetworkTransform with Owner authority will then sync this to others
        transform.position = position;
    }

    public int GetCurrentHealth()
    {
        return UseNetworkedHealth() ? networkHealth.Value : currentHealth;
    }

    public int GetMaxHealth()
    {
        return starterHealth;
    }

    // ============ Audio (for entities without PlayerControls) ============

    /// <summary>
    /// Plays the attack sound from the Power component, broadcast to all clients.
    /// Used by enemies and other non-player entities.
    /// </summary>
    public void PlayPowerSoundNetworked(float volume = 1f)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && IsSpawned)
        {
            PlayPowerSoundClientRpc(volume);
        }
        else if (NetworkManager.Singleton == null)
        {
            // Offline mode - get sound from Power component
            var power = GetComponent<Power>();
            if (power != null && power.GetAttackSound() != null)
            {
                AudioSource.PlayClipAtPoint(power.GetAttackSound(), transform.position, volume);
            }
        }
    }

    [ClientRpc]
    private void PlayPowerSoundClientRpc(float volume)
    {
        // Each client gets the sound from their local Power component (same prefab = same AudioClip)
        var power = GetComponent<Power>();
        if (power != null && power.GetAttackSound() != null)
        {
            AudioSource.PlayClipAtPoint(power.GetAttackSound(), transform.position, volume);
        }
    }
}