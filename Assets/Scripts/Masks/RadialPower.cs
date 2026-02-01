using UnityEngine;

public class RadialPower : Power
{
    [SerializeField] private int numBullets = 8;
    protected override void DoBehavior()
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