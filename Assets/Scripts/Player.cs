using UnityEngine;

public class Player : ClientBase
{
    public PlayerData PlayerData;

    public static Player Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        character = CharacterBus.Instance.character;
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

