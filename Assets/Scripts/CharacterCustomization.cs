using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCustomization : MonoBehaviour
{
    public TMP_InputField input;

    public Character character;

    private void Start()
    {
        if(character == null)
        {
            character = new Character();
        }
    }


    public void SetName()
    {
        character.characterInfo.name = input.text;
    }

    public void FinalizeCharacter()
    {
        CharacterBus.Instance.character = character;
        SceneManager.LoadScene("MainMenu");

    }
}
