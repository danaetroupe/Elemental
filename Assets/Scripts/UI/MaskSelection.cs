using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MaskSelection : NetworkBehaviour
{
    [System.Serializable]
    public class MaskButton
    {
        public Button button;
        public GameObject maskPrefab;
    }

    [SerializeField] private List<MaskButton> maskButtons = new List<MaskButton>();
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI readyText;
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color unavailableColor = Color.red;

    // Track which mask each client selected (clientId -> mask index)
    private NetworkVariable<int> player1MaskIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<int> player2MaskIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<ulong> player1ClientId = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<ulong> player2ClientId = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Track ready status
    private NetworkVariable<bool> player1Ready = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> player2Ready = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private int localSelectedMaskIndex = -1;

    void Start()
    {
        // Setup mask button listeners
        for (int i = 0; i < maskButtons.Count; i++)
        {
            int index = i; // Capture for lambda
            maskButtons[i].button.onClick.AddListener(() => OnMaskButtonClicked(index));
        }

        // Setup ready button listener
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(OnReadyButtonClicked);
            readyButton.interactable = false; // Can't ready until mask selected
        }

        // Subscribe to network variable changes
        player1MaskIndex.OnValueChanged += (oldVal, newVal) => UpdateUIState();
        player2MaskIndex.OnValueChanged += (oldVal, newVal) => UpdateUIState();
        player1Ready.OnValueChanged += OnReadyStateChanged;
        player2Ready.OnValueChanged += OnReadyStateChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        UpdateUIState();
        UpdateReadyText();
    }

    void OnMaskButtonClicked(int maskIndex)
    {
        if (!IsClient) return;

        RequestMaskSelectionServerRpc(NetworkManager.Singleton.LocalClientId, maskIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestMaskSelectionServerRpc(ulong clientId, int maskIndex)
    {
        // Determine if this is player 1 or player 2
        bool isPlayer1 = false;
        bool isPlayer2 = false;

        if (player1ClientId.Value == clientId)
        {
            isPlayer1 = true;
        }
        else if (player2ClientId.Value == clientId)
        {
            isPlayer2 = true;
        }
        else if (player1ClientId.Value == ulong.MaxValue)
        {
            // First player to join
            player1ClientId.Value = clientId;
            isPlayer1 = true;
        }
        else if (player2ClientId.Value == ulong.MaxValue)
        {
            // Second player to join
            player2ClientId.Value = clientId;
            isPlayer2 = true;
        }

        // Check if the mask is already taken by the other player
        if (isPlayer1 && player2MaskIndex.Value == maskIndex)
        {
            return; // Mask already taken by player 2
        }
        if (isPlayer2 && player1MaskIndex.Value == maskIndex)
        {
            return; // Mask already taken by player 1
        }

        // Update the player's selection
        if (isPlayer1)
        {
            player1MaskIndex.Value = maskIndex;
            player1Ready.Value = false; // Reset ready when changing mask
        }
        else if (isPlayer2)
        {
            player2MaskIndex.Value = maskIndex;
            player2Ready.Value = false; // Reset ready when changing mask
        }
    }

    void OnReadyButtonClicked()
    {
        if (!IsClient) return;

        RequestReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestReadyServerRpc(ulong clientId)
    {
        if (player1ClientId.Value == clientId)
        {
            player1Ready.Value = !player1Ready.Value;
        }
        else if (player2ClientId.Value == clientId)
        {
            player2Ready.Value = !player2Ready.Value;
        }

        // Check if both players are ready
        if (player1Ready.Value && player2Ready.Value)
        {
            EquipMasksClientRpc();
        }
    }

    void OnReadyStateChanged(bool oldVal, bool newVal)
    {
        UpdateReadyText();
        UpdateUIState();
    }

    void UpdateReadyText()
    {
        if (readyText == null) return;

        int readyCount = 0;
        if (player1Ready.Value) readyCount++;
        if (player2Ready.Value) readyCount++;

        readyText.text = $"{readyCount}/2 Players ready";
    }

    void UpdateUIState()
    {
        if (!IsClient || NetworkManager.Singleton == null) return;

        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        bool isPlayer1 = player1ClientId.Value == localClientId;
        bool isPlayer2 = player2ClientId.Value == localClientId;

        // Update each mask button
        for (int i = 0; i < maskButtons.Count; i++)
        {
            bool isSelectedByPlayer1 = player1MaskIndex.Value == i;
            bool isSelectedByPlayer2 = player2MaskIndex.Value == i;
            bool isSelectedByMe = (isPlayer1 && isSelectedByPlayer1) || (isPlayer2 && isSelectedByPlayer2);
            bool isSelectedByOther = (isPlayer1 && isSelectedByPlayer2) || (isPlayer2 && isSelectedByPlayer1);

            Image buttonImage = maskButtons[i].button.GetComponent<Image>();

            if (isSelectedByMe)
            {
                buttonImage.color = selectedColor;
                maskButtons[i].button.interactable = true;
                localSelectedMaskIndex = i;
            }
            else if (isSelectedByOther)
            {
                buttonImage.color = unavailableColor;
                maskButtons[i].button.interactable = false;
            }
            else
            {
                buttonImage.color = availableColor;
                maskButtons[i].button.interactable = true;
            }
        }

        // Update ready button state
        if (readyButton != null)
        {
            bool hasSelection = localSelectedMaskIndex >= 0;
            readyButton.interactable = hasSelection;

            bool isLocalPlayerReady = (isPlayer1 && player1Ready.Value) || (isPlayer2 && player2Ready.Value);
            TextMeshProUGUI buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isLocalPlayerReady ? "Unready" : "Ready";
            }
        }
    }

    [ClientRpc]
    void EquipMasksClientRpc()
    {
        SetMask();
    }

    public void SetMask()
    {
        // Find the local player's network object
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        // Get all player objects in the scene
        foreach (var playerObject in FindObjectsOfType<NetworkObject>())
        {
            if (playerObject.CompareTag("Player") && playerObject.OwnerClientId == localClientId)
            {
                // Determine which mask this player selected
                int maskIndex = -1;
                if (player1ClientId.Value == localClientId)
                {
                    maskIndex = player1MaskIndex.Value;
                }
                else if (player2ClientId.Value == localClientId)
                {
                    maskIndex = player2MaskIndex.Value;
                }

                if (maskIndex >= 0 && maskIndex < maskButtons.Count)
                {
                    GameObject newMask = Instantiate(maskButtons[maskIndex].maskPrefab);

                    if (playerObject.TryGetComponent<PlayerControls>(out var playerControls))
                    {
                        playerControls.EquipMask(newMask);
                    }
                }
                break;
            }
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < maskButtons.Count; i++)
        {
            maskButtons[i].button.onClick.RemoveAllListeners();
        }

        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
        }
    }
}