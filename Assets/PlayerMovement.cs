using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            SubmitPositionServerRpc(transform.position);
        }
    }

    [ServerRpc]
    private void SubmitPositionServerRpc(Vector3 position)
    {
        //Update the position on the server.
        UpdatePositionClientRpc(position);
    }

    [ClientRpc]
    private void UpdatePositionClientRpc(Vector3 position)
    {
        //Update the position on all clients.
        if (!IsOwner) transform.position = position;
    }
}