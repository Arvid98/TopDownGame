using UnityEngine;
using Unity.Netcode;

public class ChestTrigger : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private NetworkObject localPlayer;


    private Chest chest;

    private void Awake()
    {
        chest = GetComponent<Chest>();
        if (chest == null)
        {
         
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        NetworkObject netObj = collision.GetComponentInParent<NetworkObject>();
        if (netObj != null)
        {
            Debug.Log("Hittade NetworkObject med OwnerClientId: " + netObj.OwnerClientId + ". IsLocalPlayer: " + netObj.IsLocalPlayer);
        }

        if (netObj != null && netObj.IsLocalPlayer)
        {
            isPlayerInRange = true;
            localPlayer = netObj;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        NetworkObject netObj = collision.GetComponentInParent<NetworkObject>();
        if (netObj != null && netObj.IsLocalPlayer)
        {
            isPlayerInRange = false;
            if (chest.currentUser.Value == netObj.OwnerClientId)
            {
                chest.RequestCloseChestServerRpc();
            }
            localPlayer = null;
        }
    }

    private void Update()
    {
        if (isPlayerInRange && localPlayer != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                chest.RequestOpenChestServerRpc();
            }
        }
    }
}
