using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class PlayerPosition : NetworkBehaviour
{
    private Vector3 lastKnownPosition;
    private Positions allPlayerPositions;

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

        List<ulong> clientsList = new List<ulong>();
        List<Vector3> positionList = new List<Vector3>();

        
        foreach (var pos in clientPositions)
        {
            clientsList.Add(pos.Key);
            positionList.Add(pos.Value);
        }
        

        Positions positions = new Positions
        {
            clientId = clientsList.ToArray(),
            position = positionList.ToArray(),
        };


        SendAllPositionClientRpc(positions);
    }

    [ClientRpc]
    private void SendAllPositionClientRpc(Positions _positions)
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
        Dictionary<ulong, Vector3> playerPositions = new Dictionary<ulong, Vector3>();

        if (allPlayerPositions != null)
        {
            for (int i = 0; i < allPlayerPositions.clientId.Length; i++)
            {
                playerPositions[allPlayerPositions.clientId[i]] = allPlayerPositions.position[i];
            }
        }

        return playerPositions;
    }

    #endregion
}

public class Positions : INetworkSerializable
{
    public ulong[] clientId;
    public Vector3[] position;
   

    void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref position);

    }
}
