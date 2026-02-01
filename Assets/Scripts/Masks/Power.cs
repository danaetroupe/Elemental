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

    public void UsePower(Vector3 aimTarget = default)
    {
        if (IsReady())
        {
            DoBehavior(aimTarget);
            lastUseTime = Time.time;
        }
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
        }
        else
        {
            Debug.Log("No projectile added");
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