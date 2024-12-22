using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
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
    public TMP_InputField chatInputField;
    public GameObject messagePrefab;
    public Transform messageArea;

    private float currentRange = 50f;

    private bool inputIsSelected;
    private CharacterSheet characterSheet;

    private PlayerPosition playerPositionClass;

    private void Start()
    {
        playerPositionClass = GetComponentInParent<PlayerPosition>();
        characterSheet = GetComponentInParent<CharacterSheet>();
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

                DisplayMessage(newMessage);
                GetValidRecipients(newMessage); 
                chatInputField.text = "";
            }
        }
    }

    public void InputFieldSelected(bool _value)
    {
        inputIsSelected = _value;
    }

    void DisplayMessage(Message _message)
    {
        GameObject newMessageObj = Instantiate(messagePrefab, messageArea);
        newMessageObj.GetComponentInChildren<TMP_Text>().text = $"{_message.language} | {_message.user}- {_message.chatMessage}";
    }

    public void GetValidRecipients(Message _message)
    {
        playerPositionClass.AskServerForPosition(playerPosition =>
        {
            playerPositionClass.AskServerForAllPositions(() =>
            {
                Dictionary<ulong, Vector3> allPlayerPositions = playerPositionClass.GetAllPlayerPositions();
                Debug.Log(gameObject.name + " can send to " + allPlayerPositions.Count + " at " + allPlayerPositions.Keys + " " + allPlayerPositions.Values);
                Dictionary<ulong, Vector3> playersToSendTo = new Dictionary<ulong, Vector3>();

                foreach (KeyValuePair<ulong, Vector3> player in allPlayerPositions)
                {
                    if (player.Key == NetworkManager.Singleton.LocalClientId)
                        continue;

                    float distance = Vector3.Distance(playerPosition, player.Value);
                    Debug.Log("Checking player " + player.Key + ": Distance = " + distance + ", Range = " + currentRange);
                    if (distance <= currentRange)
                    {
                        Debug.Log(player.Key + " is in range at " + player.Value);
                        playersToSendTo.Add(player.Key, player.Value);
                    }
                    else
                    {
                        Debug.Log(player.Key + " is NOT in range at " + player.Value);
                    }
                }

                Debug.Log("Target Players: " + playersToSendTo);
                foreach (KeyValuePair<ulong, Vector3> targetPlayer in playersToSendTo)
                {
                    Debug.Log("Sending message to client " + targetPlayer.Key + " at position " + targetPlayer.Value);

                    SendMessageToServerRPC(_message, targetPlayer.Key);

                }
                
            });
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMessageToServerRPC(Message _message, ulong _client)
    {
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { _client }
            }
        };
        Debug.Log("Server recieved message, forwarding. .");
        SendMessageToClientRpc(_message, clientRpcParams);

    }

    [ClientRpc]
    private void SendMessageToClientRpc(Message _message, ClientRpcParams clientRpcParams = default)
    {
        
            Debug.Log(gameObject.name + " heard someone saying " + _message);
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


