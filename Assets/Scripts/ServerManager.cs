using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class ServerManager : MonoBehaviour
{
    private Dictionary<ulong, CustomizationDataNet> playerCustomizations = new Dictionary<ulong, CustomizationDataNet>();

    public static ServerManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (Application.isBatchMode) 
        {
            Debug.Log("Running as Dedicated Server");
            NetworkManager.Singleton.StartServer();

            
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            Debug.Log("Running as Client");
            //NetworkManager.Singleton.StartClient();
        }
    }

   
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        
        foreach (var kvp in playerCustomizations)
        {
            ulong existingPlayerId = kvp.Key;
            CustomizationDataNet characterData = kvp.Value;
            
            var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(existingPlayerId);
            if (playerObject != null)
            {
                var customizationApply = playerObject.GetComponent<PlayerCustomizationApply>();
                customizationApply?.ApplyCustomizationToClientRpc(characterData, playerObject.NetworkObjectId, existingPlayerId, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { clientId }
                    }
                });
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
       
        playerCustomizations.Remove(clientId);
    }
    
    public void UpdatePlayerCustomization(ulong clientId, CustomizationDataNet customizationData)
    {
        if (playerCustomizations.ContainsKey(clientId))
        {
            playerCustomizations[clientId] = customizationData;
        }
        else
        {
            playerCustomizations.Add(clientId, customizationData);
        }
    }
    
    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
