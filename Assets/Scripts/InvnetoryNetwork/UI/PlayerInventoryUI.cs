using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private InventorySystem playerInventory; 
    [SerializeField] private Transform iconContainer;          
    [SerializeField] private GameObject itemIconPrefab;           

    private void Update()
    {
        UpdateInventoryUI();
    }

    private void UpdateInventoryUI()
    {
        
        foreach (Transform child in iconContainer)
        {
            Destroy(child.gameObject);
        }

        List<InventoryItemData> inventory = playerInventory.GetInventory();
        foreach (var item in inventory)
        {
            GameObject iconGO = Instantiate(itemIconPrefab, iconContainer);
            ItemIconUI iconUI = iconGO.GetComponent<ItemIconUI>();
            if (iconUI != null)
            {

                Sprite itemSprite = ItemIconDatabase.Instance.GetSpriteForItem(item.itemName.ToString());
                iconUI.SetItem(itemSprite, item.quantity);
            }
        }
    }
}
