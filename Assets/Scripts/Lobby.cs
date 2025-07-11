using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    public string serverIP = "173.216.172.185";
    public ushort serverPort = 7777;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Connect()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = serverIP;
        transport.ConnectionData.Port = serverPort;
        Debug.Log($"Attempting to connect to {serverIP}:{serverPort}");
        NetworkManager.Singleton.StartClient();
    }

    public void Server()
    {


        NetworkManager.Singleton.StartServer();
        Debug.Log("Started Server");

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        var connectionData = transport.ConnectionData;

        string serverIp = connectionData.Address;
        ushort serverPort = connectionData.Port;

        Debug.Log($"Server IP: {serverIp}");
        Debug.Log($"Server Port: {serverPort}");

        NetworkManager.Singleton.SceneManager.LoadScene("Test", UnityEngine.SceneManagement.LoadSceneMode.Single);

    }
}
