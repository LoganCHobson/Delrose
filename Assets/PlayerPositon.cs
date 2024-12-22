using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerPosition : NetworkBehaviour
{
    private static Dictionary<ulong, Vector3> cachedPositions = new Dictionary<ulong, Vector3>();
    public static event Action PositionsUpdated;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        UpdateCachedPosition(clientId, NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.position);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        cachedPositions.Remove(clientId);
        PositionsUpdated?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPositionUpdateServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        Vector3 position = NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.transform.position;
        UpdateCachedPosition(senderId, position);
        SendPositionUpdateClientRpc(senderId, position);
    }

    [ClientRpc]
    private void SendPositionUpdateClientRpc(ulong clientId, Vector3 position)
    {
        UpdateCachedPosition(clientId, position);
    }

    private static void UpdateCachedPosition(ulong clientId, Vector3 position)
    {
        cachedPositions[clientId] = position;
        PositionsUpdated?.Invoke();
    }

    public static Dictionary<ulong, Vector3> GetAllPositions()
    {
        return new Dictionary<ulong, Vector3>(cachedPositions);
    }

    public static Vector3 GetPosition(ulong clientId)
    {
        return cachedPositions.TryGetValue(clientId, out Vector3 position) ? position : Vector3.zero;
    }

    public void RequestUpdate()
    {
        if (IsOwner)
        {
            RequestPositionUpdateServerRpc();
        }
    }
}
