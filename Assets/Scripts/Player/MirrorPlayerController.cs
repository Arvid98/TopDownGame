using UnityEngine;
using Mirror;

public class MirrorPlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        if (!isLocalPlayer) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // Send movement to the server
        CmdMove(moveX, moveY);
    }

    [Command]
    void CmdMove(float x, float y)
    {
        // Update position on the server
        transform.Translate(new Vector3(x, y, 0) * moveSpeed * Time.deltaTime);

        // Sync the new position to all clients
        RpcUpdatePosition(transform.position);
    }

    [ClientRpc]
    void RpcUpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}