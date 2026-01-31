using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent OnPlayerEnter;
    [SerializeField] private UnityEvent OnPlayerExit;
    [SerializeField] private UnityEvent OnItemInteraction;
    [SerializeField] private UnityEvent OnItemDisable;

    [Header("Settings")]
    [SerializeField] private Key KEYBIND = Key.E;

    private bool isInteracting = false;
    private bool isTriggered = false;
    private string playerTag = "Player";
    private bool isInRange = false;

    void Update()
    {
        if (isTriggered && Keyboard.current != null && Keyboard.current[KEYBIND].wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag(playerTag))
        {
            isInRange = true;
            OnPlayerEnter.Invoke();
            isTriggered = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag(playerTag))
        {
            isInRange = false;
            OnPlayerExit.Invoke();
            isTriggered = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (gameObject.CompareTag(playerTag))
        {
            isInRange = true;
            OnPlayerEnter.Invoke();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            isInRange = false;
            OnPlayerExit.Invoke();
        }
    }

    public void Interact()
    {
        isInteracting = !isInteracting;
        if (isInteracting)
        {
            OnItemInteraction.Invoke();
        }
        else
        {
            OnItemDisable.Invoke();
        }
    }

    // Public methods for external scripts
    public bool IsPlayerInRange()
    {
        return isInRange;
    }
}