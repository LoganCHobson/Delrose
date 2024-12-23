using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerCustomizationApply : NetworkBehaviour
{
    public GameObject gfx;

    void Start()
    {
        if (IsOwner)
        {
            string customizationFilePath = PlayerPrefs.GetString("SelectedCharacterFilePath", "");

            if (string.IsNullOrEmpty(customizationFilePath))
            {
                Debug.LogError("No character customization file path found!");
                return;
            }

            CustomizationData characterData = LoadCustomizationData(customizationFilePath);

            if (characterData == null)
            {
                Debug.LogError("Failed to load character customization data.");
                return;
            }

            CustomizationDataNet characterDataNet = ConvertToCustomizationDataNet(characterData);

            UpdateServerRpc(characterDataNet);
        }
    }

    CustomizationData LoadCustomizationData(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<CustomizationData>(json);
        }
        else
        {
            Debug.LogError("Customization file does not exist at path: " + filePath);
            return null;
        }
    }

    CustomizationDataNet ConvertToCustomizationDataNet(CustomizationData characterData)
    {
        CustomizationDataNet characterDataNet = new CustomizationDataNet();
        
        characterDataNet.modelCustomizationData = new ModelCustomizationDataNet
        {
            color = characterData.modelCustomizationData.color
        };
       
        characterDataNet.characterCustomizationData = new CharacterInfoNet
        {
            name = characterData.characterCustomizationData.name,
            age = characterData.characterCustomizationData.age,
            race = characterData.characterCustomizationData.race,
            gender = characterData.characterCustomizationData.gender,
            bio = characterData.characterCustomizationData.bio,
            knownLanguages = characterData.characterCustomizationData.knownLanguages.ToArray(),
        };

        return characterDataNet;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdateServerRpc(CustomizationDataNet characterData, ServerRpcParams _param = default)
    {
        NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(_param.Receive.SenderClientId);
        ulong networkObjectId = playerObject.NetworkObjectId;
        
        ApplyCustomizationOnServer(characterData, playerObject, _param.Receive.SenderClientId);
        
        ApplyCustomizationToClientRpc(characterData, networkObjectId, _param.Receive.SenderClientId);

        ServerManager.Instance.UpdatePlayerCustomization(_param.Receive.SenderClientId, characterData);
    }

    private void ApplyCustomizationOnServer(CustomizationDataNet characterData, NetworkObject _playerObject, ulong senderClientId)
    {
        if (_playerObject != null)
        {
            ApplyCustomizationToPlayer(_playerObject, characterData);
        }
        else
        {
            Debug.LogError("Player object not found on server.");
        }
    }
   
    [ClientRpc]
    public void ApplyCustomizationToClientRpc(CustomizationDataNet characterData, ulong networkObjectId, ulong playerId, ClientRpcParams _param = default)
    {
        NetworkObject playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        if (playerObject != null)
        {
            ApplyCustomizationToPlayer(playerObject, characterData);
        }
    }
   
    private void ApplyCustomizationToPlayer(NetworkObject playerObject, CustomizationDataNet characterData)
    {
        PlayerCustomizationApply playerCustomization = playerObject.GetComponent<PlayerCustomizationApply>();
        if (playerCustomization != null)
        {
            playerCustomization.ApplyCustomization(characterData);
        }
        else
        {
            Debug.LogError("No customization component found on player object!");
        }
    }

    public void ApplyCustomization(CustomizationDataNet characterData)
    {
        if (gfx != null)
        {
            gfx.GetComponent<Renderer>().material.color = characterData.modelCustomizationData.color;
            CharacterSheet temp = GetComponent<CharacterSheet>();
            temp.character.name = characterData.characterCustomizationData.name;
            temp.character.age = characterData.characterCustomizationData.age;
            temp.character.race = characterData.characterCustomizationData.race;
            temp.character.gender = characterData.characterCustomizationData.gender;
            temp.character.bio = characterData.characterCustomizationData.bio;
            temp.character.knownLanguages = characterData.characterCustomizationData.knownLanguages.ToList();
        }
    }
}


[System.Serializable]
public struct CustomizationDataNet : INetworkSerializable
{
    public ModelCustomizationDataNet modelCustomizationData;
    public CharacterInfoNet characterCustomizationData;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        modelCustomizationData.NetworkSerialize(serializer);
        characterCustomizationData.NetworkSerialize(serializer);
    }
}

[System.Serializable]
public struct ModelCustomizationDataNet : INetworkSerializable
{
    public Color color;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        float r = color.r, g = color.g, b = color.b, a = color.a;
        serializer.SerializeValue(ref r);
        serializer.SerializeValue(ref g);
        serializer.SerializeValue(ref b);
        serializer.SerializeValue(ref a);
        color = new Color(r, g, b, a); 
    }
}

[System.Serializable]
public struct CharacterInfoNet : INetworkSerializable
{
    public string name;
    public int age;
    public string race;
    public string gender;
    public string bio;
    public Languages[] knownLanguages;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref age);
        serializer.SerializeValue(ref race);
        serializer.SerializeValue(ref gender);
        serializer.SerializeValue(ref bio);
       
        serializer.SerializeValue(ref knownLanguages);
    }
}


