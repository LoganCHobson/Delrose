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

    public Ranges currentRange;

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
        playerPositionClass.AskServerForPosition();
        Vector3 playerPosition = playerPositionClass.GetLastKnownPosition();

        
        playerPositionClass.AskServerForAllPositions();

        
        Dictionary<ulong, Vector3> allPlayerPositions = playerPositionClass.GetAllPlayerPositions();

        Dictionary<ulong, Vector3> playersToSendTo = new Dictionary<ulong, Vector3>();

        foreach (KeyValuePair<ulong, Vector3> player in allPlayerPositions)
        {
            if (player.Key == NetworkManager.Singleton.LocalClientId)
            {
                continue;
            }

            float distance = Vector3.Distance(playerPosition, player.Value);

            if (distance <= (int)currentRange)
            {
                playersToSendTo.Add(player.Key, player.Value);
            }
        }

        foreach (KeyValuePair<ulong, Vector3> targetPlayer in playersToSendTo)
        {
           
            SendMessageToClientRpc(targetPlayer.Key, _message);
        }
    }

    [ClientRpc]
    private void SendMessageToClientRpc(ulong _clientId, Message _message, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
           
            DisplayMessage(_message);
        }
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


