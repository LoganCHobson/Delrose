using UnityEngine;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine.SceneManagement;
public class ServerListObjManager : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text popText;
    public TMP_Text accessibilityText;
    public TMP_Text pingText;
    public Server server;

    private float timeBetweenClicks = 0.3f; 
    private float lastClickTime = -1f; 
    public void SetValues(Server _server)
    {
        server = _server;
       
        nameText.text = _server.ui.serverName.ToString();
        popText.text = _server.ui.serverPop.ToString();
        accessibilityText.text = _server.ui.accessibility.ToString();
        pingText.text = _server.ui.ping.ToString();
    }

    public void MaintainValues()
    {
        //Don't have a way to do this rn.
    }

    private void Update()
    {
        //MaintainValues();
    }

    public void OnButtonClick() //Doing a double click.
    {
        if (Time.time - lastClickTime <= timeBetweenClicks)
        {
            Connect();
        }
        else
        {
            lastClickTime = Time.time;
        }
    }

    public void Connect()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetConnectionData(server.ip, server.port);

        NetworkManager.Singleton.StartClient();

        if (NetworkManager.Singleton.IsClient)
        {
            SceneManager.LoadScene("MovementTest");
        }
    }
}

