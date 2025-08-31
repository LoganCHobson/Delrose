using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    public GameObject content;
    public GameObject messagePrefab;
    public TMP_InputField inputField;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }


    public void SendChatMessage()
    {
        if (inputField.text != string.Empty)
        {
            SendMessageToServerRpc(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
        }

    }

    [ServerRpc(RequireOwnership = false)]
    void SendMessageToServerRpc(string message, ServerRpcParams rpcParams = default) //Code ran on the server
    {
        SendMessageClientRpc(message);
    }

    [ClientRpc]
    void SendMessageClientRpc(string message) //Code ran on the client.
    {
        AddMessage(message);
    }

    public void AddMessage(string message)
    {
        GameObject temp = Instantiate(messagePrefab, content.transform);
        temp.GetComponentInChildren<TMP_Text>().text = message;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !inputField.isFocused)
        {
            Canvas canvas = GetComponentInChildren<Canvas>(true);

            if (canvas.isActiveAndEnabled)
            {
                canvas.gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {

                canvas.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }

        }

        if (inputField != null && inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage();
        }
    }
}
