using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Text inventoryText;

    public void UpdateInventoryUI(InventorySystem inventorySystem)
    {
        inventoryText.text = "";

        foreach (var item in inventorySystem.GetInventory())
        {
            inventoryText.text += $"{item.itemName.ToString()} x{item.quantity}\n";
        }
    }
}
