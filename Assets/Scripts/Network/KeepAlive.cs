using Unity.Netcode;
using UnityEngine;

public class KeepAlive : NetworkBehaviour
{
    private float timer = 0f;

    private void Update()
    {
        if (IsClient)
        {
            timer += Time.deltaTime;
            if (timer > 1f)
            {
                SendKeepAliveServerRpc();
                timer = 0f;
            }
        }
    }

    [ServerRpc]
    private void SendKeepAliveServerRpc()
    {

    }
}
