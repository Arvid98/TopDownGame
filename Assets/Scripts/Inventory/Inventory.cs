using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public InventoryUIManager inventoryUIManager; // Länk till UI-manager
    public List<ItemInstance> items = new List<ItemInstance>();

    private void Awake()
    {
        inventoryUIManager = FindAnyObjectByType<InventoryUIManager>();
    }

    public void AddItem(Item item, int quantity)
    {

        ItemInstance existingItem = items.Find(i => i.itemData == item);

        if (existingItem != null && item.isStackable)
        {
            existingItem.quantity += quantity;
        }
        else
        {
            items.Add(new ItemInstance(item, quantity));
        }

        Debug.Log($"Added {quantity} {item.itemName} to inventory");

        inventoryUIManager.UpdateInventoryUI();
    }

    public void RemoveItem(Item item, int quantity)
    {
        ItemInstance existingItem = items.Find(i => i.itemData == item);

        if (existingItem != null)
        {
            if (existingItem.quantity > quantity)
            {
                existingItem.quantity -= quantity;
            }
            else
            {
                items.Remove(existingItem);
            }

            Debug.Log($"Removed {quantity} {item.itemName} from inventory");
        }

        inventoryUIManager.UpdateInventoryUI();
    }
}
