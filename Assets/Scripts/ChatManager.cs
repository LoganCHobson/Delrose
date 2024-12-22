using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public enum Languages
{
    COMMON,
    ELVISH,
    DWARVISH,
    ORCISH,
}

public enum Ranges
{
    WHISPER = 5,
    QUIET = 10,
    INDOORS = 20,
    NORMAL = 50,
    OUTDOORS = 100,
    SHOUT = 200,
    DM = 500,
    EVENT = 1000,
    LOCAL = 50,
    GLOBAL = 9999,
}

public class ChatManager : NetworkBehaviour
{
    private PlayerNetworkManager playerNetworkManager;

    public TMP_InputField chatInputField;
    public GameObject messagePrefab;
    public Transform messageArea;

    private Ranges currentRange = Ranges.NORMAL;

    private bool inputIsSelected;
    private CharacterSheet characterSheet;

    private List<PlayerPosData> playerPosDataList = new List<PlayerPosData>();
    private ulong myID;

    private void Start()
    {
        playerNetworkManager = GetComponentInParent<PlayerNetworkManager>();
        
        //characterSheet = GetComponentInParent<CharacterSheet>();

    }
    private void Update()
    {
        if (inputIsSelected && !string.IsNullOrEmpty(chatInputField.text))
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Message newMessage = new Message
                {
                    range = Ranges.NORMAL,
                    language = "Common",
                    user = "Solar",
                    chatMessage = chatInputField.text
                };

                //DisplayMessage(newMessage);
                GetValidRecipients(newMessage);
                chatInputField.text = "";
            }
        }
    }

    public void InputFieldSelected(bool _value)
    {
        inputIsSelected = _value;
    }

    public void DisplayMessage(Message _message)
    {
        GameObject newMessageObj = Instantiate(messagePrefab, messageArea);
        newMessageObj.GetComponentInChildren<TMP_Text>().text = $"{_message.language} | {_message.user}- {_message.chatMessage}";
    }


    public void GetValidRecipients(Message _message)
    {
        GetPlayersServerRPC();
       
        Vector3 myPos = playerPosDataList.Find(p => p.clientId == myID).position;
        Debug.Log($"My position: {myPos}");

        List<ulong> targetPlayers = new List<ulong>();

        foreach (PlayerPosData player in playerPosDataList)
        {
            Debug.Log($"Player {player.clientId} position: {player.position}");
            float distance = Vector3.Distance(myPos, player.position);
            if (distance <= (float)currentRange)
            {
                targetPlayers.Add(player.clientId);
                Debug.Log($"The distance between {myID} and {player.clientId} is {distance} for the range {currentRange}");
            }
            else
            {
                Debug.Log($"{player.clientId} is out of range of {myID}");
                Debug.Log($"The distance between {myID} and {player.clientId} is {distance} for the range {(float)currentRange}");
            }
        }

        SendMessageServerRPC(targetPlayers.ToArray(), _message);
    }


    [ServerRpc(RequireOwnership = false)]
    public void GetPlayersServerRPC(ServerRpcParams _param = default)
    {
        ulong senderId = _param.Receive.SenderClientId; //Gets who asked.
        //Vector3 position = NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.transform.position; //Gets one 

        List<PlayerPosData> allPlayerData = new List<PlayerPosData>();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList) //Gets all
        {
            allPlayerData.Add(new PlayerPosData
            {
                clientId = client.ClientId,
                position = client.PlayerObject.transform.position
            });

            Debug.Log("Server says " + client.ClientId + " is at " + client.PlayerObject.transform.position);
        }


        SendPositionUpdateClientRpc(senderId, allPlayerData.ToArray(), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } } }); //Sends back to who asked.
    }

    [ClientRpc]
    private void SendPositionUpdateClientRpc(ulong _clientId, PlayerPosData[] _allPlayerData, ClientRpcParams _param)
    {
        playerPosDataList = _allPlayerData.ToList<PlayerPosData>();
        myID = _clientId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRPC(ulong[] _targetPlayers, Message _message, ServerRpcParams _param = default)
    {
        SendMessageClientRPC(_message, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = _targetPlayers
            }
        });
    }

    [ClientRpc]
    public void SendMessageClientRPC(Message _message, ClientRpcParams _param = default)
    {
        DisplayMessage(_message);
    }

}

public class Message : INetworkSerializable
{
    public string language;
    public string user;
    public string chatMessage;
    public Ranges range;

    void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref range);
        serializer.SerializeValue(ref language);
        serializer.SerializeValue(ref user);
        serializer.SerializeValue(ref chatMessage);
    }
}

public struct PlayerPosData : INetworkSerializable
{
    public ulong clientId;
    public Vector3 position;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref position);
    }
}
