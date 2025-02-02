using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class TestUI : MonoBehaviour
{
    [SerializeField] private Button addItemButton;
    [SerializeField] private Button removeItemButton;
    [SerializeField] private InventoryUI inventoryUI;

    private InventoryManager inventoryManager;

    private IEnumerator Start()
    {
      
        while (NetworkManager.Singleton == null ||
               NetworkManager.Singleton.SpawnManager == null ||
               NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject() == null)
        {
            yield return null;
        }

   
        GameObject localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().gameObject;
        inventoryManager = localPlayer.GetComponent<InventoryManager>();

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager saknas på den lokala spelaren!");
        }

        if (addItemButton != null)
        {
            addItemButton.onClick.AddListener(() =>
            {
                if (inventoryManager != null)
                    inventoryManager.RequestAddItem("Sword", 1);
            });
        }
        else
        {
            Debug.LogError("addItemButton är inte tilldelad i Inspector!");
        }

        if (removeItemButton != null)
        {
            removeItemButton.onClick.AddListener(() =>
            {
                if (inventoryManager != null)
                    inventoryManager.RequestRemoveItem("Sword", 1);
            });
        }
        else
        {
            Debug.LogError("removeItemButton är inte tilldelad i Inspector!");
        }
    }

    private void Update()
    {
      
        if (inventoryUI != null && inventoryManager != null)
        {
            InventorySystem invSystem = inventoryManager.GetComponent<InventorySystem>();
            inventoryUI.UpdateInventoryUI(invSystem);
        }
    }
}
