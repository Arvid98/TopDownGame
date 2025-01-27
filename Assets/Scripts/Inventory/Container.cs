using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public List<ItemInstance> containerItems = new List<ItemInstance>();

    public void AddToContainer(Item item, int quantity)
    {
        ItemInstance existingInstance = containerItems.Find(i => i.itemData == item);

        if (existingInstance != null && item.isStackable)
        {
            existingInstance.quantity += quantity;
        }
        else
        {
            containerItems.Add(new ItemInstance(item, quantity));
        }
    }

    public void RemoveFromContainer(Item item, int quantity)
    {
        ItemInstance existingInstance = containerItems.Find(i => i.itemData == item);

        if (existingInstance != null)
        {
            if (existingInstance.quantity > quantity)
            {
                existingInstance.quantity -= quantity;
            }
            else
            {
                containerItems.Remove(existingInstance);
            }
        }
    }
}
