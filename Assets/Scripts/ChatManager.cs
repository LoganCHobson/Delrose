using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebSocketSharp;

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

    private PlayerMovement playerMovement;

    public TMP_InputField chatInputField;
    public GameObject messagePrefab;
    public Transform messageArea;

    private List<Message> messageHistory = new List<Message>();
    private int chatHistoryIndex;

    public Ranges currentRange = Ranges.NORMAL;
    public Languages currentLanguage = Languages.COMMON;

    private List<PlayerPosData> playerPosDataList = new List<PlayerPosData>();
    private ulong myID;

    private bool isReady;

    

    private void Start()
    {
        playerNetworkManager = GetComponentInParent<PlayerNetworkManager>();

        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void Update()
    {
        if (chatInputField.isFocused)
        {
            playerMovement.paused = true;

            if (!string.IsNullOrEmpty(chatInputField.text))
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    string inputText = chatInputField.text;
                    Message newMessage = new Message
                    {
                        range = currentRange,
                        language = currentLanguage.ToString(),
                        user = "Solar",
                        chatMessage = ParseMessage(inputText)
                    };
                    messageHistory.Add(newMessage);
                    chatHistoryIndex++;
                    GetValidRecipients(newMessage);
                    chatInputField.text = "";
                }
            }

            if (messageHistory.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    chatInputField.text = messageHistory[Mathf.Clamp(++chatHistoryIndex, 0, messageHistory.Count - 1)].chatMessage;
                    return;
                }


                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    chatInputField.text = messageHistory[Mathf.Clamp(--chatHistoryIndex, 0, messageHistory.Count - 1)].chatMessage;
                    return;
                }
            }
        }
        else
        {
            playerMovement.paused = false;
        }
    }


    public void DisplayMessage(Message _message)
    {
        GameObject newMessageObj = Instantiate(messagePrefab, messageArea);
        TMP_Text messageText = newMessageObj.GetComponentInChildren<TMP_Text>();

        messageText.text = $"{_message.language} | {_message.user}- {_message.chatMessage}";

       
        AddLinkHandler(messageText);
    }

    private void AddLinkHandler(TMP_Text messageText)
    {
        messageText.GetComponentInParent<Canvas>().gameObject.AddComponent<GraphicRaycaster>();

        var linkHandler = messageText.gameObject.AddComponent<ChatLinkHandler>();
        linkHandler.Setup(messageText, OnMessageClick);
    }

    private string ParseMessage(string message)
    {
        //[PlayerName=AnimationName]
        string pattern = @"\[(.*?)=(.*?)\]";
        return System.Text.RegularExpressions.Regex.Replace(message, pattern, match =>
        {
            string playerName = match.Groups[1].Value; //EX "Billy waves"
            string animationName = match.Groups[2].Value;  //EX "wave1"
            
            return $"<link={animationName}><u><color=blue>{playerName}</color></u></link>";
        });
    }

    private void OnMessageClick(TMP_Text messageText)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(messageText, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = messageText.textInfo.linkInfo[linkIndex];
            string action = linkInfo.GetLinkID();
            
            HandleAction(action);
        }
    }

    private void HandleAction(string action)
    {
        Debug.Log($"Triggering action: {action}");
       
        if (action == "wave1")
        {
            PlayAnimation("Wave");
        }
        else if (action == "dance1")
        {
            PlayAnimation("Dance");
        }
        else
        {
            Debug.LogWarning($"Unknown action: {action}");
        }

        
    }

    private void PlayAnimation(string animationName)
    {
        Debug.Log($"Playing animation: {animationName}");

        
    }


    public void GetValidRecipients(Message _message)
    {
        isReady = false;
        GetPlayersServerRPC();

        StartCoroutine(WaitForPositionData(() =>
        {
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
        }));
    }
    private IEnumerator WaitForPositionData(System.Action onReady)
    {
        yield return new WaitUntil(() => isReady); 
        onReady?.Invoke(); 
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
        isReady = true;
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

public class ChatLinkHandler : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text messageText;
    private System.Action<TMP_Text> onClick;

    public void Setup(TMP_Text text, System.Action<TMP_Text> clickAction)
    {
        messageText = text;
        onClick = clickAction;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(messageText);
    }
}