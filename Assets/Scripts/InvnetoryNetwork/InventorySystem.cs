using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class InventorySystem : NetworkBehaviour
{
    private NetworkList<InventoryItemData> inventory = new NetworkList<InventoryItemData>();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            inventory.OnListChanged += OnInventoryChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            inventory.OnListChanged -= OnInventoryChanged;
        }
    }

    public void AddItem(string itemName, int quantity)
    {
        if (IsServer)
        {
            int index = GetItemIndex(itemName);
            if (index >= 0)
            {
                var item = inventory[index];
                item.quantity += quantity;
                inventory[index] = item;
            }
            else
            {
                inventory.Add(new InventoryItemData
                {
                    itemName = new FixedString32Bytes(itemName),
                    quantity = quantity
                });
            }
        }
    }

    public void RemoveItem(string itemName, int quantity)
    {
        if (IsServer)
        {
            int index = GetItemIndex(itemName);
            if (index >= 0)
            {
                var item = inventory[index];
                item.quantity -= quantity;

                if (item.quantity <= 0)
                {
                    inventory.RemoveAt(index);
                }
                else
                {
                    inventory[index] = item;
                }
            }
        }
    }

    private int GetItemIndex(string itemName)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].itemName.ToString() == itemName)
            {
                return i;
            }
        }
        return -1; 
    }

    private void OnInventoryChanged(NetworkListEvent<InventoryItemData> changeEvent)
    {
        Debug.Log($"Inventory updated: {changeEvent.Type}");
    }

    public NetworkList<InventoryItemData> GetInventory()
    {
        return inventory;
    }
}
