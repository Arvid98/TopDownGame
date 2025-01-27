using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    public Inventory playerInventory; 
    public Item testItem;           
    public int quantity = 1;        

    void Start()
    {
        if (testItem != null && playerInventory != null)
        {
            playerInventory.AddItem(testItem, quantity);
            Debug.Log($"Added {quantity} {testItem.itemName} to inventory");
        }
        else
        {
            Debug.LogWarning("TestItem or PlayerInventory is not assigned");
        }
    }
}
