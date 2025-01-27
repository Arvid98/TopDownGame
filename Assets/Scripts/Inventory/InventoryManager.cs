using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    Inventory playerInventory;

    private void Awake()
    {
        playerInventory = GetComponent<Inventory>();
    }
    public void TransferItemToContainer(Item item, Container container)
    {
        ItemInstance itemInstance = playerInventory.items.Find(i => i.itemData == item);

        if (itemInstance != null)
        {
            int quantityToRemove = itemInstance.quantity;

            playerInventory.RemoveItem(item, quantityToRemove);

            container.AddToContainer(item, quantityToRemove); 
        }
    }


    public void TransferItemToInventory(Item item, Container container)
    {
        ItemInstance itemInstance = container.containerItems.Find(i => i.itemData == item);

        if (itemInstance != null)
        {
            int quantityToRemove = itemInstance.quantity;

            container.RemoveFromContainer(item, quantityToRemove); 

            playerInventory.AddItem(item, quantityToRemove);
        }
    }



}
