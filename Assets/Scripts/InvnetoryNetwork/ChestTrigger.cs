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
            Debug.LogError("Chest-komponenten hittades inte på " + gameObject.name + ". Se till att Chest-scriptet är tillagt!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter kallad av: " + other.gameObject.name);

        NetworkObject netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj != null)
        {
            Debug.Log("Hittade NetworkObject med OwnerClientId: " + netObj.OwnerClientId + ". IsLocalPlayer: " + netObj.IsLocalPlayer);
        }

        if (netObj != null && netObj.IsLocalPlayer)
        {
            isPlayerInRange = true;
            localPlayer = netObj;
            Debug.Log("Local player är i närheten av chestet. Tryck på E för att öppna chestet.");
        }
    }


    private void OnTriggerExit(Collider other)
    {
        NetworkObject netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj != null && netObj.IsLocalPlayer)
        {
            isPlayerInRange = false;
          
            if (chest.currentUser.Value == netObj.OwnerClientId)
            {
                chest.RequestCloseChestServerRpc();
            }
            localPlayer = null;
            Debug.Log("Local player lämnade chest-zonen.");
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
