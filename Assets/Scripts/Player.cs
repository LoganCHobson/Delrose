using UnityEngine;

public class Player : ClientBase
{
    public PlayerData PlayerData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0))
        { 
        }
    }
}

public struct PlayerData
{
    public ulong clientId;


}

