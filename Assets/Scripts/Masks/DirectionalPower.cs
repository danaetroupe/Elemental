using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class DirectionalPower : Power
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 direction;

    protected override void DoBehavior()
    {
        Vector3 screenPosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 worldPosition = hit.point;
            SpawnProjectile(worldPosition + offset, direction);
        } 
    }
}
