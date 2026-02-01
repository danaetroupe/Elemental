using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class MaskPickup : NetworkBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private Collider pickupTrigger;
    [SerializeField] private GameObject maskPrefab;

    private bool consumed;

    private void Reset()
    {
        pickupTrigger = GetComponent<Collider>();
        if (pickupTrigger != null) pickupTrigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || consumed) return;

        var player = other.GetComponentInParent<PlayerControls>();
        if (player == null) return;

        consumed = true;

        if (pickupTrigger != null) pickupTrigger.enabled = false;

        ulong pickerClientId = player.OwnerClientId;
        Debug.Log($"[MaskPickup] Picked up by clientId={pickerClientId} (server).");

        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { pickerClientId }
            }
        };

        EquipMaskClientRpc(pickerClientId, rpcParams);

        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void EquipMaskClientRpc(ulong pickerClientId, ClientRpcParams rpcParams = default)
    {
        if (maskPrefab == null)
        {
            Debug.LogError("[MaskPickup] maskPrefab is not assigned on the pickup.");
            return;
        }

        var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayerObj == null)
        {
            Debug.LogError("[MaskPickup] Could not find local player object.");
            return;
        }

        var player = localPlayerObj.GetComponent<PlayerControls>();
        if (player == null)
        {
            Debug.LogError("[MaskPickup] Local player has no PlayerControls component.");
            return;
        }

        var newMask = Instantiate(maskPrefab);
        player.EquipMask(newMask);

        Debug.Log($"[MaskPickup] Equipped mask '{maskPrefab.name}' for clientId={pickerClientId}.");
    }
}