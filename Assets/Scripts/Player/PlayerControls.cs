using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerControls : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] private GameObject cameraRig;

    [Header("Cooldowns")]
    [SerializeField] float dodgeCooldown = 1f;
    [SerializeField] float attackCooldown = 0.5f;
    
    private Vector2 moveInput;
    private float lastDodgeTime = -1f;
    private float lastAttackTime = -1f;

    private GameObject currentMask;
    private List<GameObject> maskInventory = new List<GameObject>();

    private SpriteRenderer spriteRenderer;
    private Animator animator;


    private void Awake()
    {
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
                if (maskController != null)
                {
                    maskController.UsePower();
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
        currentMask.transform.parent = gameObject.transform;

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

        // TODO: Change character appearance based on maskController.characterColor
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
        if (!IsOwner) return; 
            HandleMovement();
    }

    private void HandleMovement()
    {
        ReadMovementInput();
        ApplyMovement();
    }
    private void ReadMovementInput()
    {
        moveInput = Vector2.zero;
        
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            moveInput.y = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            moveInput.y = -1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveInput.x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveInput.x = 1f;

        // Normalize to prevent faster diagonal movement
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();
    }
    private void ApplyMovement()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }
    public override void OnNetworkSpawn()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
            playerInput.enabled = IsOwner;

        if (cameraRig != null)
            cameraRig.SetActive(IsOwner);
    }
}
