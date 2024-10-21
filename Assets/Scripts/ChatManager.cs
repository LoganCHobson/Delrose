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

    private bool inputIsSelected;
    private CharacterSheet characterSheet;

    private void Start()
    {
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

                SendMessageServerRpc(newMessage);
                chatInputField.text = "";
            }
        }
    }

    

    public void InputFieldSelected(bool value)
    {
        inputIsSelected = value;
    }

    void DisplayMessage(Message message)
    {
        GameObject newMessageObj = Instantiate(messagePrefab, messageArea);
        newMessageObj.GetComponentInChildren<TMP_Text>().text = $"{message.language} | {message.user}- {message.chatMessage}";
    }

    [ServerRpc(RequireOwnership = false)]
    void SendMessageServerRpc(Message message, ulong clientId = default) //So this is interesting. This sends a message up to the server.
    {

        SendMessageClientRpc(message);
        
            
    }

    [ClientRpc]
    void SendMessageClientRpc(Message message) //Will send to all clients and have them run DisplayMessage on their end.
    {
        DisplayMessage(message);
    }
}


class Message : INetworkSerializable //So because we decided to be cool and to try to do things with classes, we gotta tell the server how to use it.
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

