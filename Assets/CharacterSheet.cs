using System.Collections.Generic;
using UnityEngine;

public class CharacterSheet : MonoBehaviour
{
    public string characterName;
    public int characterAge;
    public string characterGender;
    public string bio;
    public List<Languages> languages = new List<Languages>();

    public Character character;

    private void Start()
    {
        character = new Character
        {
            name = characterName,
            age = characterAge,
            gender = characterGender,
            bio = bio,
            knownLanguages = languages,

        };

    }

    private void Update()
    {
        if (character.name == null)
        {
            character.name = characterName;
        }
    }

}

public class Character
{
    public string name;
    public int age;
    public string gender;
    public string bio;
    public List<Languages> knownLanguages;
}
