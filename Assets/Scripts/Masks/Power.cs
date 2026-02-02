using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public abstract class Power : MonoBehaviour
{

    [SerializeField] float cooldownTime = 1f;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] GameObject projectilePrefab;

    [Header("Audio")]
    [SerializeField] protected AudioClip attackSound;
    [SerializeField] protected float attackSoundVolume = 1f;

    [Header("Debug")]
    protected float lastUseTime = -999f;

    public void UsePower(Vector3 aimTarget = default)
    {
        if (IsReady())
        {
            Debug.Log($"[Power] Firing power '{GetType().Name}' on '{gameObject.name}'");
            DoBehavior(aimTarget);
            lastUseTime = Time.time;
            PlayAttackSound();
        }
        else
        {
            Debug.Log($"[Power] Power '{GetType().Name}' on cooldown. Remaining: {GetRemainingCooldown():F2}s");
        }
    }

    protected virtual void PlayAttackSound()
    {
        if (attackSound == null) return;
        
        // Try to find PlayerControls in parent hierarchy (mask is attached to player)
        var playerControls = GetComponentInParent<PlayerControls>();
        if (playerControls != null)
        {
            // Use networked sound - broadcasts to all clients
            playerControls.PlayAttackSoundNetworked(attackSound, attackSoundVolume);
        }
        else
        {
            // Fallback for non-player usage (e.g., testing)
            AudioSource.PlayClipAtPoint(attackSound, transform.position, attackSoundVolume);
        }
    }

    /// <summary>
    /// Returns the attack sound clip for network sync purposes.
    /// </summary>
    public AudioClip GetAttackSound()
    {
        return attackSound;
    }

    protected abstract void DoBehavior(Vector3 aimTarget);

    public virtual bool IsReady()
    {
        return Time.time >= lastUseTime + cooldownTime;
    }

    public virtual float GetRemainingCooldown()
    {
        float remaining = (lastUseTime + cooldownTime) - Time.time;
        return Mathf.Max(0, remaining);
    }

    protected virtual GameObject SpawnProjectile(Vector3 origin, Vector3 direction)
    {
        GameObject projectile;

        if (projectilePrefab != null)
        {
            projectile = Instantiate(projectilePrefab, origin, Quaternion.identity);
            Debug.Log($"[Power] Spawned projectile '{projectilePrefab.name}' at {origin}");
        }
        else
        {
            Debug.LogError($"[Power] No projectilePrefab assigned on '{gameObject.name}'! Cannot spawn projectile.");
            return null;
        }

        if (!projectile.TryGetComponent<Projectile>(out var projectileScript))
        {
            Debug.Log("Projectile has no projectlie script!");
        }

        projectileScript.Initialize(direction.normalized, projectileSpeed);


        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsServer &&
            projectile.TryGetComponent<NetworkObject>(out var netObj) &&
            !netObj.IsSpawned)
        {
            netObj.Spawn();
        }

        return projectile;
    }
}