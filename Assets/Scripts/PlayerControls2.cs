using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerControls2 : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] private GameObject cameraRig;
    [SerializeField] private MeshRenderer meshRenderer;

    private readonly NetworkVariable<Color> playerColor = new NetworkVariable<Color>(writePerm: NetworkVariableWritePermission.Server);


    private Vector2 moveInput;

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
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

    void FixedUpdate()
    {
        if (!IsOwner) return; 
            HandleMovement();
    }

    public override void OnNetworkSpawn()
    {

        playerColor.OnValueChanged += (_, newColor) => ApplyColor(newColor);

        if (IsServer)
        {
            playerColor.Value = (OwnerClientId == 0) ? Color.blue : Color.red;
        }
        ApplyColor(playerColor.Value);


        if (cameraRig != null)
            cameraRig.SetActive(IsOwner);
    }

    private void ApplyColor(Color c)
    {
        if (meshRenderer == null) return;

        var mpb = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", c);
        meshRenderer.SetPropertyBlock(mpb);
    }

}
