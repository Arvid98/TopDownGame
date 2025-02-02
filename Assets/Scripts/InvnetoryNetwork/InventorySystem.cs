using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class InventorySystem : NetworkBehaviour
{
  
    private NetworkList<InventoryItemData> inventory;

    private void Awake()
    {
        inventory = new NetworkList<InventoryItemData>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            inventory.OnListChanged += HandleInventoryChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            inventory.OnListChanged -= HandleInventoryChanged;
        }
    }

    
    public void AddItem(string itemName, int quantity)
    {
        if (!IsServer)
        {
          
            return;
        }

        int index = GetItemIndex(itemName);
        if (index >= 0)
        {
            var item = inventory[index];
            item.quantity += quantity;
            inventory[index] = item;
        }
        else
        {
            InventoryItemData newItem = new InventoryItemData
            {
                itemName = new FixedString32Bytes(itemName),
                quantity = quantity
            };
            inventory.Add(newItem);
        }
    }


    public void RemoveItem(string itemName, int quantity)
    {
        if (!IsServer)
        {
          
            return;
        }

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

    public bool TryRemoveItem(string itemName, int quantity)
    {
        int index = GetItemIndex(itemName);
        if (index >= 0)
        {
            var item = inventory[index];
            if (item.quantity >= quantity)
            {
                item.quantity -= quantity;
                if (item.quantity <= 0)
                {
                    inventory.RemoveAt(index);
                }
                else
                {
                    inventory[index] = item;
                }
                return true;
            }
        }
        return false;
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

    private void HandleInventoryChanged(NetworkListEvent<InventoryItemData> changeEvent)
    {
     
    }


    public List<InventoryItemData> GetInventory()
    {
        List<InventoryItemData> list = new List<InventoryItemData>();
        for (int i = 0; i < inventory.Count; i++)
        {
            list.Add(inventory[i]);
        }
        return list;
    }
}
