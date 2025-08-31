using Unity.Netcode;
using UnityEngine;
public class ClientBase : NetworkBehaviour
{
    public Character character;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            InitOwnership();
            InitPlayer();
        }
    }

    private void InitOwnership()
    {

        Debug.Log("Adjusting Ownership. .");
        GetComponentInChildren<Camera>(true).gameObject.SetActive(true);
        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<Player>().enabled = true;


    }

    private void InitPlayer()
    {
        character = CharacterBus.Instance.character;
        UpdateServerReplicaCustomizationServerRpc(character);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateServerReplicaCustomizationServerRpc(Character character)
    {

    }

}
