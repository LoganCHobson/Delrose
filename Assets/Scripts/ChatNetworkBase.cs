using System.Collections.Generic;
using Unity.Netcode;

public abstract class ChatNetworkBase : NetworkBehaviour
{

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageToServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        List<ulong> targetClientIds = new List<ulong>();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {


            targetClientIds.Add(client.ClientId);
        }

        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClientIds.ToArray()
            }
        };

        SendMessageClientRpc(message, clientRpcParams);
    }

    [ClientRpc]
    void SendMessageClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        AddMessage(message);
    }

    public abstract void AddMessage(string _message);
}
