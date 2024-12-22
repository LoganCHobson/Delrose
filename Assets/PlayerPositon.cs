using System.Collections;
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
        Debug.Log("Server says your position is: " + transform.position);
        SendPositionClientRpc(transform.position);
    }

    [ClientRpc]
    private void SendPositionClientRpc(Vector3 _position)
    {
        if (IsOwner)
        {
            Debug.Log(gameObject.name + " says: i heard from server my position is: " + transform.position);
            lastKnownPosition = _position;
        }
    }

    public void AskServerForPosition(System.Action<Vector3> callback = null)
    {
        if (IsOwner)
        {
            RequestPositionServerRpc();
            StartCoroutine(WaitForLastKnownPosition(callback));
        }
    }

    private IEnumerator WaitForLastKnownPosition(System.Action<Vector3> callback)
    {
        while (lastKnownPosition == Vector3.zero)
        {
            yield return null; // Wait until lastKnownPosition is updated
        }
        callback?.Invoke(lastKnownPosition);
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
            Debug.Log("Server say that the client ids are as follows: " + client.Key);
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
            Debug.Log("Server says: All player positions are as follows: " + pos.Key + " " + pos.Value);
        }
        

        Positions positions = new Positions
        {
            clientId = clientsList.ToArray(),
            position = positionList.ToArray(),
        };

        for (int i = 0; i < positions.clientId.Length; i++)
        {
            Debug.Log("Server says that the stuff is packaged and that the positions are as follows: " + positions.clientId[i] + " " + positions.position[i]);
        }
        SendAllPositionClientRpc(positions);
    }

    [ClientRpc]
    private void SendAllPositionClientRpc(Positions _positions)
    {
        if (IsOwner)
        {
            allPlayerPositions = _positions;
            for (int i = 0; i < _positions.clientId.Length; i++)
            {
                Debug.Log(gameObject.name + " heard from server that the positions are as follows: " + _positions.clientId[i] + " " + _positions.position[i]);
            }
        }
        
    }

    public void AskServerForAllPositions(System.Action callback = null)
    {
        if (IsOwner)
        {
            RequestAllClientsPositionServerRpc();
            StartCoroutine(WaitForAllPlayerPositions(callback));
        }
    }

    private IEnumerator WaitForAllPlayerPositions(System.Action callback)
    {
        while (allPlayerPositions == null || allPlayerPositions.clientId == null)
        {
            yield return null; // Wait until positions are populated
        }
        callback?.Invoke();
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
        foreach (var kvp in playerPositions)
        {
            Debug.Log($"Player {kvp.Key} Position: {kvp.Value}");
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
