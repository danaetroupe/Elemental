using System;
using Unity.VisualScripting;
using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    public event Action OnTriggerActivated;

    [SerializeField] private string playerTag = "Player"; // Tag to identify the player

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag(playerTag))
        {
            hasTriggered = true;
            OnTriggerActivated?.Invoke();
        }
    }

    // For 3D games, use OnTriggerEnter instead
    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag(playerTag))
        {
            hasTriggered = true;
            OnTriggerActivated?.Invoke();
        }
    }
}