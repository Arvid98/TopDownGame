[System.Serializable]
public class ItemInstance
{
    public Item itemData; 
    public int quantity;  

    public ItemInstance(Item item, int qty)
    {
        itemData = item;
        quantity = qty;
    }

    public bool AddToStack(int amount)
    {
        if (itemData.isStackable && quantity + amount <= itemData.maxStackSize)
        {
            quantity += amount;
            return true;
        }
        return false;
    }
}
