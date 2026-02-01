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

    [Header("Debug")]
    protected float lastUseTime = -999f;

    public  void UsePower()
    {
        if (IsReady())
        {
            DoBehavior();
            lastUseTime = Time.time;
        }
    }

    protected abstract void DoBehavior();

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
        }
        else
        {
            Debug.Log("No projectile added");
            return null;
        }

        // Add or get the Projectile component
        if (!projectile.TryGetComponent<Projectile>(out var projectileScript))
        {
            Debug.Log("Projectile has no projectlie script!");
        }

        // Initialize the projectile
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