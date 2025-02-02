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
            Debug.LogError("Chest-komponenten hittades inte p� " + gameObject.name + ". Se till att Chest-scriptet �r tillagt!");
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
            Debug.Log("Local player �r i n�rheten av chestet. Tryck p� E f�r att �ppna chestet.");
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
            Debug.Log("Local player l�mnade chest-zonen.");
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
