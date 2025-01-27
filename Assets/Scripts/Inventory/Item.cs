using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public int itemID;
    public Sprite icon;
    public string description;

  
    public ItemType itemType; 
    public bool isStackable;  
    public int maxStackSize; 
}

public enum ItemType
{
    Consumable,
    Equipment,
    Resource,
    KeyItem
}
