using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public bool paused;

    private void Update()
    {
        if (!IsOwner) return;

        if (!paused)
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
        }
    }
}
