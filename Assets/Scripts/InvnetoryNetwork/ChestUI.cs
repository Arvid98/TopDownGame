using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ChestUI : MonoBehaviour
{
    [SerializeField] private Chest chest; 
    [SerializeField] private TextMeshProUGUI chestText; 

    private void Start()
    {
        if (chest == null)
        {
            Debug.LogError("Chest-referensen saknas i ChestUI!");
        }
        if (chestText == null)
        {
            Debug.LogError("ChestText (TextMeshProUGUI) saknas i ChestUI!");
        }
    }

    private void Update()
    {
        if (chest != null && chestText != null)
        {
            UpdateChestUI();
        }
    }

 
    private void UpdateChestUI()
    {
        chestText.text = "";

        List<InventoryItemData> inventory = chest.GetChestInventory();
        if (inventory.Count == 0)
        {
            chestText.text = "Chest är tomt.";
        }
        else
        {
            foreach (var item in inventory)
            {
                chestText.text += $"{item.itemName.ToString()} x{item.quantity}\n";
            }
        }
    }
}
