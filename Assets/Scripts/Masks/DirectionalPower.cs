using System.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

public class DirectionalPower : Power
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 direction;

    protected override void DoBehavior()
    {
        SpawnProjectile(transform.position + offset, direction);
    }
}
