using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI inventoryText;

   
    public void UpdateInventoryUI(InventorySystem inventorySystem)
    {
        if (inventorySystem == null || inventoryText == null)
            return;

        inventoryText.text = "";
        foreach (var item in inventorySystem.GetInventory())
        {
            inventoryText.text += $"{item.itemName.ToString()} x{item.quantity}\n";
        }
    }
}
