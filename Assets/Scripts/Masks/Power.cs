using System;
using System.Collections;
using UnityEngine;

public abstract class Power : MonoBehaviour
{
    [Header("Power Settings")]
    public Sprite projectileSprite;
    public float cooldownTime = 1f;

    [Header("Projectile Settings")]
    public float projectileSpeed = 10f;
    public float projectileLifetime = 5f;
    public GameObject projectilePrefab;

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

        return projectile;
    }

}

public class RadialPower : Power
{
    [SerializeField] private int numBullets = 8;
    public override void UsePower()
    {
        float angleStep = 360f / numBullets;

        for (int i = 0; i < numBullets; i++)
        {
            float angle = i * angleStep;

            float angleInRadians = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angleInRadians), 0, Mathf.Sin(angleInRadians));

            GameObject projectile = SpawnProjectile(transform.position, direction);
        }
    }
}