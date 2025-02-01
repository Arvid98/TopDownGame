using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public InventoryUI inventoryUI;
    public TMP_Text inventoryDisplay;
    public Button addItemButton;
    public Button removeItemButton;

    void Start()
    {
        //addItemButton.onClick.AddListener(() => inventoryManager.RequestAddItem("Sword", 1));
        //removeItemButton.onClick.AddListener(() => inventoryManager.RequestRemoveItem("Sword", 1));
    }

    void Update()
    {
        if (inventoryUI != null && inventoryManager.inventorySystem != null)
        {
            inventoryUI.UpdateInventoryUI(inventoryManager.inventorySystem);
           
        }
    }
    public void AddItem()
    {
        inventoryManager.RequestAddItem("Sword", 1);
    }

    public void RemoveItem()
    {
        inventoryManager.RequestRemoveItem("Sword", 1);
    }
}
