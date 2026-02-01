using UnityEngine;
using UnityEngine.InputSystem;

public class RadialPower : Power
{
    [SerializeField] private int numBullets = 8;
    [Header("Spawn Offset")]
    [SerializeField] private float spawnRadius = 2f;
    protected override void DoBehavior()
    {
        float angleStep = 360f / numBullets;

        for (int i = 0; i < numBullets; i++)
        {
            float angle = i * angleStep;

            float angleInRadians = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angleInRadians), 0, Mathf.Sin(angleInRadians));

            // Spawn slightly away from the center so bullets don't overlap the shooter.
            Vector3 spawnPos = transform.position + direction.normalized * Mathf.Max(0f, spawnRadius);
            GameObject projectile = SpawnProjectile(spawnPos, direction);
        }
    }
}