using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class DirectionalPower : Power
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 direction;

    protected override void DoBehavior(Vector3 aimTarget)
    {
        SpawnProjectile(aimTarget + offset, direction);
    }
}
