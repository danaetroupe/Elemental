using System;
using System.Collections;
using UnityEngine;

public abstract class Power : MonoBehaviour
{

    [SerializeField] float cooldownTime = 1f;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] GameObject projectilePrefab;

    [Header("Debug")]
    protected float lastUseTime = -999f;

    public abstract void UsePower();

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
            // Use the projectile prefab if assigned
            // Preserve the prefab's authored rotation (Quaternion.identity would override it)
            projectile = Instantiate(projectilePrefab, origin, projectilePrefab.transform.rotation);
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

        return projectile;
    }
}