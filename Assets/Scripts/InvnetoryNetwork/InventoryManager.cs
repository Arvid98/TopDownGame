using Unity.Netcode;
using UnityEngine;

public class InventoryManager : NetworkBehaviour
{
    private InventorySystem inventorySystem;

    private void Awake()
    {
        inventorySystem = GetComponent<InventorySystem>();
        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem saknas på " + gameObject.name);
        }
    }

 
    public void RequestAddItem(string itemName, int quantity)
    {
        SubmitAddItemServerRpc(itemName, quantity);
    }

  
    public void RequestRemoveItem(string itemName, int quantity)
    {
        SubmitRemoveItemServerRpc(itemName, quantity);
    }


    [ServerRpc(RequireOwnership = true)]
    private void SubmitAddItemServerRpc(string itemName, int quantity, ServerRpcParams rpcParams = default)
    {
        inventorySystem.AddItem(itemName, quantity);
        Debug.Log($"Added {itemName} x{quantity} to inventory of client {OwnerClientId}");
    }

    [ServerRpc(RequireOwnership = true)]
    private void SubmitRemoveItemServerRpc(string itemName, int quantity, ServerRpcParams rpcParams = default)
    {
        inventorySystem.RemoveItem(itemName, quantity);
        Debug.Log($"Removed {itemName} x{quantity} from inventory of client {OwnerClientId}");
    }
}
