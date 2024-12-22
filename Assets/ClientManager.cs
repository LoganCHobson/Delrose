using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public string ServerIP = "127.0.0.1"; //server's IP
    public ushort ServerPort = 7777;      //server's port

    void Start()
    {
        JoinServer();
    }

    public void JoinServer()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ServerIP, ServerPort);
        NetworkManager.Singleton.StartClient();
    }
}
