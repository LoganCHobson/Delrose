using System.Collections.Generic;
using UnityEngine;

public class ServerListManager : MonoBehaviour
{
    public List<Server> serverList = new List<Server>();
    public GameObject serverContent;
    public GameObject serverListPrefab;
    public float refreshTimer = 30f;

    public void Start()
    {
        //serverList = GetServers();

        Server devServer = new Server
        {
            ui = new ServerUI
            {
                serverName = "Dev_Server",
                serverPop = "0/0",
                accessibility = Accessibility.PUBLIC,
                ping = 70,
            },

            ip = "127.0.0.1",
            port = 7777


        };
        serverList.Add(devServer);
        RefreshUI();
    }


    private void Update()
    {
        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0)
        {
            //GetServers();

            RefreshUI();
            refreshTimer = 30f;
        }

    }





    public List<Server> GetServers()
    {
        //Some sort of code to get servers.
        Debug.Log("Servers aquired");
        List<Server> servers = new List<Server>();
        return servers;

    }

    public void RefreshUI()
    {
        foreach (Transform obj in serverContent.transform)
        {
            Destroy(obj.gameObject); //Remove all old.
        }

        foreach (Server server in serverList)
        {

            GameObject temp = Instantiate(serverListPrefab, serverContent.transform); //Make new.
            temp.GetComponent<ServerListObjManager>().SetValues(server);
        }
    }
}

public struct Server
{
    public ServerUI ui;

    public string ip;
    public ushort port;
}

public struct ServerUI
{
    public string serverName;
    public string serverPop;
    public Accessibility accessibility;
    public int ping;
}


public enum Accessibility
{
    PUBLIC,
    PRIVATE,
}
