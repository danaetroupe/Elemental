using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PlayerControls : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] private GameObject cameraRig;

    [Header("Cooldowns")]
    [SerializeField] float dodgeCooldown = 1f;
    [SerializeField] float attackCooldown = 0.5f;
    

    [SerializeField] private SpriteRenderer sprite;

    private Vector2 moveInput;
    private float lastDodgeTime = -1f;
    private float lastAttackTime = -1f;

    private GameObject currentMask;
    private List<GameObject> maskInventory = new List<GameObject>();

    private GameObject maskVisualInstance;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private NetworkAnimator networkAnimator;

    private readonly NetworkVariable<bool> facingRight = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);


    private void Awake()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
        if (sprite == null)
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
        }
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
                if (networkAnimator != null)
                {
                    networkAnimator.SetTrigger("Dodge");
                }
                else
                {
                    animator.SetTrigger("Dodge");
                }
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
                if (networkAnimator != null)
                {
                    networkAnimator.SetTrigger("Attack");
                }
                else
                {
                    animator.SetTrigger("Attack");
                }
            }

            TryUseEquippedPower();
        }
    }

    private void TryUseEquippedPower()
    {
        if (currentMask == null) return;

        var maskController = currentMask.GetComponent<MaskController>();
        if (maskController == null) return;

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            maskController.UsePower();
            return;
        }

        if (IsServer)
        {
            maskController.UsePower();
        }
        else if (IsOwner)
        {
            UseMaskPowerServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void UseMaskPowerServerRpc()
    {
        if (currentMask == null) return;

        var maskController = currentMask.GetComponent<MaskController>();
        if (maskController == null) return;

        maskController.UsePower();
    }

    public void EquipMaskServerOnly(GameObject maskPrefab)
    {
        if (!IsServer) return;
        if (maskPrefab == null) return;

        var newMask = Instantiate(maskPrefab);
        EquipMask(newMask);
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
        else
        {
            maskController.HideSprite();
        }

        if (currentMask != null)
        {
            if (!maskInventory.Contains(currentMask))
            {
                maskInventory.Add(currentMask);
            }
        }

        currentMask = newMask;

        currentMask.transform.SetParent(transform);
        currentMask.transform.localPosition = Vector3.zero;
        currentMask.transform.localRotation = Quaternion.identity;

        if (!maskInventory.Contains(newMask))
        {
            maskInventory.Add(newMask);
        }

        SwapCharacter();

        Debug.Log($"Equipped mask: {newMask.name}");
    }


    public void AttachMaskVisual(GameObject maskPrefab)
    {
        if (maskPrefab == null) return;


        if (IsOwner) return;

        if (maskVisualInstance != null)
        {
            Destroy(maskVisualInstance);
            maskVisualInstance = null;
        }

        maskVisualInstance = Instantiate(maskPrefab);
        maskVisualInstance.transform.SetParent(transform);
        maskVisualInstance.transform.localPosition = Vector3.zero;
        maskVisualInstance.transform.localRotation = Quaternion.identity;

        foreach (var col in maskVisualInstance.GetComponentsInChildren<Collider>(true))
            col.enabled = false;

        foreach (var rb in maskVisualInstance.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
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
        HandleAnimation();
    }
    private void HandleAnimation()
    {
        if (!animator)
            return;
        animator.SetFloat("moveInputX", moveInput.x);
        animator.SetFloat("moveInputY", moveInput.y);
        animator.SetFloat("isMovingX", Mathf.Abs(moveInput.x));
        animator.SetFloat("isMovingY", Mathf.Abs(moveInput.y));
        
        if (moveInput.x != 0)
        {
            bool shouldFaceRight = moveInput.x > 0;
            ApplySpriteFlip(shouldFaceRight);
            if (IsOwner && IsSpawned && facingRight.Value != shouldFaceRight)
            {
                facingRight.Value = shouldFaceRight;
            }
        }
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

        facingRight.OnValueChanged += HandleFacingChanged;
        if (IsOwner && sprite != null)
        {
            facingRight.Value = sprite.flipX;
        }
        else
        {
            ApplySpriteFlip(facingRight.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        facingRight.OnValueChanged -= HandleFacingChanged;
    }

    private void HandleFacingChanged(bool previousValue, bool newValue)
    {
        ApplySpriteFlip(newValue);
    }

    private void ApplySpriteFlip(bool faceRight)
    {
        if (sprite == null)
        {
            return;
        }

        sprite.flipX = faceRight;
    }
}
