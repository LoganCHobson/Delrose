using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkManager : NetworkBehaviour
{
    public ChatManager chatManager;

    [ServerRpc]
    public void SendMessageServerRpc(Message message, ulong targetClientId, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { targetClientId }
            }
        };
        SendMessageClientRpc(message, clientRpcParams);
    }

    [ClientRpc]
    private void SendMessageClientRpc(Message message, ClientRpcParams clientRpcParams = default)
    {
        chatManager.DisplayMessage(message);
    }
}
