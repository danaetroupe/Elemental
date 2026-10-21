using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Transport for solo play: hosting needs no sockets, so this works in WebGL where a browser can't listen for connections.
/// </summary>
public class OfflineTransport : NetworkTransport
{
    public override ulong ServerClientId => 0;

    public override void Initialize(NetworkManager networkManager = null)
    {
    }

    public override bool StartServer()
    {
        return true;
    }

    public override bool StartClient()
    {
        Debug.LogError("[OfflineTransport] Solo play can only host.");
        return false;
    }

    public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
    {
    }

    public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
    {
        clientId = 0;
        payload = default;
        receiveTime = Time.realtimeSinceStartup;
        return NetworkEvent.Nothing;
    }

    public override void DisconnectRemoteClient(ulong clientId)
    {
    }

    public override void DisconnectLocalClient()
    {
    }

    public override ulong GetCurrentRtt(ulong clientId)
    {
        return 0;
    }

    public override void Shutdown()
    {
    }
}
