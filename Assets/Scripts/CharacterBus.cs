using UnityEngine;

public class CharacterBus : MonoBehaviour
{

    public Character character;

    public static CharacterBus Instance;
   
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
