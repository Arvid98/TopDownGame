using Unity.Netcode;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySystem inventorySystem; 

    public void RequestAddItem(string itemName, int quantity)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            inventorySystem.AddItem(itemName, quantity); // on server
        }
        else
        {
            SubmitAddItemRequestServerRpc(itemName, quantity);
        }
    }

    public void RequestRemoveItem(string itemName, int quantity)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            inventorySystem.RemoveItem(itemName, quantity); // on server
        }
        else
        {
            SubmitRemoveItemRequestServerRpc(itemName, quantity);
        }
    }

    [ServerRpc]
    private void SubmitAddItemRequestServerRpc(string itemName, int quantity)
    {
        inventorySystem.AddItem(itemName, quantity);
    }

    [ServerRpc]
    private void SubmitRemoveItemRequestServerRpc(string itemName, int quantity)
    {
        inventorySystem.RemoveItem(itemName, quantity);
    }
}
