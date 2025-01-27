using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private void Update()
    {
        if (!IsOwner) return; 

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0) * 5f * Time.deltaTime;
        transform.position += movement;

        UpdatePositionServerRpc(transform.position);
    }

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}
