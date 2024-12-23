using System.Collections.Generic;
using UnityEngine;

public class CharacterSheet : MonoBehaviour
{

    public CharacterInfo character;


}
[System.Serializable]
public class CharacterInfo
{
    public string name;
    public int age;
    public string race;
    public string gender;
    public string bio;
    public List<Languages> knownLanguages = new List<Languages>();
}
