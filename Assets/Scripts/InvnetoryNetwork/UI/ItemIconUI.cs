using UnityEngine;
using UnityEngine.UI; 


public class ItemIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text quantityText;


    public void SetItem(Sprite sprite, int quantity)
    {
        iconImage.sprite = sprite;
        if (quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }
}
