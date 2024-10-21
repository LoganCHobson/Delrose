using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField chatInputField;
    public GameObject messagePrefab;
    public Transform messageArea;

    private bool inputIsSelected;

    private void Update()
    {
        if(inputIsSelected && !string.IsNullOrEmpty(chatInputField.text))
        {
            if(Input.GetKeyDown(KeyCode.Return)) 
            {
                Message newMessage = new Message
                {
                    channel = "General", 
                    language = "Common", 
                    user = "Solar", 
                    chatMessage = chatInputField.text
                };

                SendMessage(newMessage);
                chatInputField.text = "";
            }
        }
    }
    void SendMessage(Message message)
    {
        GameObject newMessageObj = Instantiate(messagePrefab, messageArea);
        newMessageObj.GetComponentInChildren<TMP_Text>().text = $"{message.language} | {message.user}- {message.chatMessage}";
    }

    public void InputFieldSelected(bool value)
    {
        inputIsSelected = value;
    }
}




class Message
{
    public string channel;
    public string language;
    public string user;
    public string chatMessage;
}

