using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI quantityText;
    public Button useButton; // Assign in prefab or dynamically

    private InventoryItemData currentItem;
    private InventoryUseHandler useHandler;

    public void Setup(InventoryItemData item)
    {
        currentItem = item;

        iconImage.sprite = item.icon;
        nameText.text = item.itemName;
        effectText.text = item.effectDescription;
        quantityText.text = item.quantity.ToString();

        // Find the InventoryUseHandler in scene (or assign manually)
        useHandler = FindObjectOfType<InventoryUseHandler>();

        // Clear any old listeners and assign new click listener
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OnUseButtonClicked);
        }
    }

    private void OnUseButtonClicked()
    {
        if (useHandler != null && currentItem != null)
        {
            useHandler.UseItem(currentItem);
        }
    }

    // Optional: If you want to update quantity without reloading all UI
    public void UpdateQuantity()
    {
        quantityText.text = currentItem.quantity.ToString();
    }
}
