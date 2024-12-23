using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomizationManager : MonoBehaviour
{
    public GameObject prefab;

    public GameObject character;
    public Transform spawnPoint;

    public CharacterInfo characterInfo;
    public CustomizationData customizationData;
    public ModelCustomizationData modelCustomizationData;

    public GameObject customizationCanvas;

    bool edit = false;
    void Start()
    {
        GameObject temp = Instantiate(prefab);
        temp.transform.position = spawnPoint.transform.position;

        character = temp;

        if (!edit)
        {
            customizationData = new CustomizationData();
            customizationData.characterCustomizationData = new CharacterInfo();
            customizationData.modelCustomizationData = new ModelCustomizationData();
        }

    }

    public void Toggle(GameObject _obj)
    {
        _obj.SetActive(!_obj.activeInHierarchy);
        customizationCanvas.SetActive(!customizationCanvas.activeInHierarchy);
    }

    public void CharacterName(TMP_InputField inputField)
    {
        customizationData.characterCustomizationData.name = inputField.text;
    }

    public void CharacterAge(TMP_InputField inputField)
    {
        if (int.TryParse(inputField.text, out int age))
        {
            customizationData.characterCustomizationData.age = age;
        }
        else
        {
            Debug.LogWarning("Invalid age input: " + inputField.text);
        }
    }

    public void CharacterRace(TMP_InputField inputField)
    {
        customizationData.characterCustomizationData.race = inputField.text;
    }

    public void CharacterGender(TMP_InputField inputField)
    {
        customizationData.characterCustomizationData.gender = inputField.text;
    }

    public void CharacterBio(TMP_InputField inputField)
    {
        customizationData.characterCustomizationData.bio = inputField.text;
    }

    public void KnownLanguages(TMP_Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case 0:
                customizationData.characterCustomizationData.knownLanguages.Clear();
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.COMMON);
                break;

            case 1:
                customizationData.characterCustomizationData.knownLanguages.Clear();
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.COMMON);
                break;
            case 2:
                customizationData.characterCustomizationData.knownLanguages.Clear();
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.ELVISH);
                break;
            case 3:
                customizationData.characterCustomizationData.knownLanguages.Clear();
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.COMMON);
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.ELVISH);
                break;

            case 4:
                customizationData.characterCustomizationData.knownLanguages.Clear();
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.DWARVISH);
                break;

            case 5:
                customizationData.characterCustomizationData.knownLanguages.Clear();
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.COMMON);
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.DWARVISH);
                break;

            case 6:
                customizationData.characterCustomizationData.knownLanguages.Clear();
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.ELVISH);
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.DWARVISH);
                break;
            case 7:
                customizationData.characterCustomizationData.knownLanguages.Clear();
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.COMMON);
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.ELVISH);
                customizationData.characterCustomizationData.knownLanguages.Add(Languages.DWARVISH);
                break;


        }
    }



    public void Save()
    {
        customizationData.modelCustomizationData.color = character.GetComponent<Renderer>().material.color;
       

        string json = JsonUtility.ToJson(customizationData);

        string filePath = Path.Combine(Application.persistentDataPath, customizationData.characterCustomizationData.name + "_customization.json");

        File.WriteAllText(filePath, json);

        Debug.Log("Customization saved to file: " + filePath);
    }


    public void MainMenu()
    {
        SceneManager.LoadScene("ClientStartScene");
    }

    //Remove this later
    public void SelectCharacter(string characterName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, characterName + "_customization.json");
        PlayerPrefs.SetString("SelectedCharacterFilePath", filePath);
        PlayerPrefs.Save();
    }

}

[System.Serializable]
public class CustomizationData
{
    public ModelCustomizationData modelCustomizationData;
    public CharacterInfo characterCustomizationData;
}
[System.Serializable]
public class ModelCustomizationData
{
    public Color color;

}



