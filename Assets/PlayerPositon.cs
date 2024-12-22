using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerPosition : NetworkBehaviour
{
    private Vector3 lastKnownPosition;
    private Dictionary<ulong, Vector3> allPlayerPositions = new Dictionary<ulong, Vector3>();

    #region SoloRequest

    [ServerRpc(RequireOwnership = false)]
    public void RequestPositionServerRpc(ServerRpcParams rpcParams = default)
    {
        SendPositionClientRpc(transform.position);
    }

    [ClientRpc]
    private void SendPositionClientRpc(Vector3 _position)
    {
        if (IsOwner)
        {
            lastKnownPosition = _position;
        }
    }

    public void AskServerForPosition()
    {
        if (IsOwner)
        {
            RequestPositionServerRpc();
        }
    }

    public Vector3 GetLastKnownPosition()
    {
        return lastKnownPosition;
    }

    #endregion

    #region RequestAll

    [ServerRpc(RequireOwnership = false)]
    public void RequestAllClientsPositionServerRpc(ServerRpcParams rpcParams = default)
    {
        var connectedClients = NetworkManager.Singleton.ConnectedClients;
        Dictionary<ulong, Vector3> clientPositions = new Dictionary<ulong, Vector3>();

        foreach (var client in connectedClients)
        {
            var clientObject = client.Value.PlayerObject;
            if (clientObject != null)
            {
                clientPositions[client.Key] = clientObject.transform.position;
            }
        }

        

        SendAllPositionClientRpc(clientPositions);
    }

    [ClientRpc]
    private void SendAllPositionClientRpc(Dictionary<ulong, Vector3> _positions)
    {
        if (IsOwner)
        {
            allPlayerPositions = _positions;
        }
    }

    public void AskServerForAllPositions()
    {
        if (IsOwner)
        {
            RequestAllClientsPositionServerRpc();
        }
    }

    public Dictionary<ulong, Vector3> GetAllPlayerPositions()
    {
        return allPlayerPositions;
    }

    #endregion
}


