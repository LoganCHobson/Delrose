using Unity.Netcode;
using UnityEngine;
public class ClientBase : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        InitOwnership();
    }

    private void InitOwnership()
    {
        if (IsOwner)
        {
            Debug.Log("Adjusting Ownership. .");
            GetComponentInChildren<Camera>(true).gameObject.SetActive(true);
            GetComponent<PlayerMovement>().enabled = true;
        }
    }
}
