using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;

    [Header("Cooldowns")]
    [SerializeField] float dodgeCooldown = 1f;
    [SerializeField] float attackCooldown = 0.5f;
    
    private Vector2 moveInput;
    private float lastDodgeTime = -1f;
    private float lastAttackTime = -1f;

    public GameObject currentMask;
    private List<GameObject> maskInventory = new List<GameObject>();

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake()
    {
        // Get or add required components
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        animator = GetComponent<Animator>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (Time.time >= lastDodgeTime + dodgeCooldown)
        {
            lastDodgeTime = Time.time;

            if (animator != null)
            {
                animator.SetTrigger("Dodge");
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            if (currentMask != null)
            {
                MaskController maskController = currentMask.GetComponent<MaskController>();
                if (maskController != null && maskController.power != null)
                {
                    maskController.power.UsePower();
                }
            }
        }
    }

    public void EquipMask(GameObject newMask)
    {
        if (newMask == null)
        {
            Debug.LogWarning("Attempted to equip null mask!");
            return;
        }

        MaskController maskController = newMask.GetComponent<MaskController>();
        if (maskController == null)
        {
            Debug.LogWarning("Mask does not have MaskController component!");
            return;
        }

        if (currentMask != null)
        {
            if (!maskInventory.Contains(currentMask))
            {
                maskInventory.Add(currentMask);
            }
        }

        currentMask = newMask;

        if (!maskInventory.Contains(newMask))
        {
            maskInventory.Add(newMask);
        }

        SwapCharacter();

        Debug.Log($"Equipped mask: {newMask.name}");
    }
    public void SwapCharacter()
    {
        if (currentMask == null)
        {
            Debug.LogWarning("No current mask to swap character from!");
            return;
        }

        MaskController maskController = currentMask.GetComponent<MaskController>();
        if (maskController == null)
        {
            Debug.LogWarning("Current mask does not have MaskController component!");
            return;
        }

        Sprite characterSprite = maskController.characterSprite;

        if (characterSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = characterSprite;
            Debug.Log($"Swapped to character sprite: {characterSprite.name}");
        }
        else
        {
            Debug.LogWarning("Character sprite or SpriteRenderer is null!");
        }
    }

    public void AddMaskToInventory(GameObject mask)
    {
        if (mask != null && !maskInventory.Contains(mask))
        {
            maskInventory.Add(mask);
            Debug.Log($"Added mask to inventory: {mask.name}");
        }
    }

    public void RemoveMaskFromInventory(GameObject mask)
    {
        if (maskInventory.Contains(mask))
        {
            maskInventory.Remove(mask);
            Debug.Log($"Removed mask from inventory: {mask.name}");
        }
    }

    void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}
