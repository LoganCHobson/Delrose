using Unity.Netcode;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    void Start()
    {
        if (Application.isBatchMode) // This checks if the build is running as a server.
        {
            Debug.Log("Running as Dedicated Server");
            NetworkManager.Singleton.StartServer(); // Start the server.
        }
        else
        {
            Debug.Log("Running as Client");
             // Start the client.
        }
    }
}
