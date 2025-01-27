using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public Inventory playerInventory;          
    public GameObject inventorySlotPrefab;    
    public Transform content;                 

    private List<GameObject> activeSlots = new List<GameObject>();

    public void UpdateInventoryUI()
    {
        foreach (var slot in activeSlots)
        {
            Destroy(slot);
        }
        activeSlots.Clear();

        foreach (var itemInstance in playerInventory.items)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, content);
            activeSlots.Add(slot);

            Image iconImage = slot.transform.Find("Icon").GetComponent<Image>();
            iconImage.sprite = itemInstance.itemData.icon;

          
            TMP_Text quantityText = slot.transform.Find("Quantity").GetComponent<TMP_Text>();
            quantityText.text = itemInstance.itemData.isStackable ? itemInstance.quantity.ToString() : "";

        }
    }
}
