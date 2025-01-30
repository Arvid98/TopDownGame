using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI inventoryText;

    public void UpdateInventoryUI(InventorySystem inventorySystem)
    {
        inventoryText.text = "";

        foreach (var item in inventorySystem.GetInventory())
        {
            inventoryText.text += $"{item.itemName.ToString()} x{item.quantity}\n";
        }
    }
}
