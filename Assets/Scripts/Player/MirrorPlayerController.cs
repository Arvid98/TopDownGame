using UnityEngine;
using Mirror;

public class MirrorPlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;

    private float lastMoveX = 0f;
    private float lastMoveY = 0f;

    void Update()
    {
        if (!isLocalPlayer) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        float speed = new Vector2(moveX, moveY).sqrMagnitude;

        animator.SetFloat("Horizontal", moveX);
        animator.SetFloat("Vertical", moveY);
        animator.SetFloat("Speed", speed);

        if (speed > 0.01f)
        {
            if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
            {
                // Horizontal movement is stronger
                lastMoveX = (moveX > 0) ? 1 : -1;
                lastMoveY = 0;
            }
            else
            {
                // Vertical movement is stronger (or equal)
                lastMoveX = 0;
                lastMoveY = (moveY > 0) ? 1 : -1;
            }
        }

        // Use the snapped directions if not moving
        float displayX = (speed > 0.01f) ? lastMoveX : lastMoveX;
        float displayY = (speed > 0.01f) ? lastMoveY : lastMoveY;

        animator.SetFloat("Horizontal", displayX);
        animator.SetFloat("Vertical", displayY);
        animator.SetFloat("Speed", speed);


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