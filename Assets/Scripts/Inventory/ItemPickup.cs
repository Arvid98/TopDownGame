using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item itemData; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            Inventory playerInventory = other.GetComponent<Inventory>();
            if (playerInventory != null)
            {
                playerInventory.AddItem(itemData, 1); 
                Debug.Log($"Picked up {itemData.itemName}");

                Destroy(gameObject);
            }
        }
    }
}
