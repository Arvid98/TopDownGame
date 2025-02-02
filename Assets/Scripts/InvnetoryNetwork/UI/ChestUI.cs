using UnityEngine;
using System.Collections.Generic;

public class ChestUI : MonoBehaviour
{
    [SerializeField] private Chest chest;              
    [SerializeField] private Transform iconContainer;    
    [SerializeField] private GameObject itemIconPrefab;    

    private void Update()
    {
        UpdateChestUI();
    }

  
    private void UpdateChestUI()
    {
  
        foreach (Transform child in iconContainer)
        {
            Destroy(child.gameObject);
        }

        List<InventoryItemData> inventory = chest.GetChestInventory();
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
