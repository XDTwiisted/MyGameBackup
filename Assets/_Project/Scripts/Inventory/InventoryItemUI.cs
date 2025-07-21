using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI durabilityText; // Only shown if item.isDurable
    public Button useButton;

    private InventoryItemData currentItem;
    private int currentQuantity;
    private InventoryUseHandler useHandler;

    /// <summary>
    /// Setup this item UI element based on inventory item data and quantity.
    /// </summary>
    public void Setup(InventoryItemData item, int quantity)
    {
        currentItem = item;
        currentQuantity = quantity;

        // Set visuals
        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (nameText != null)
            nameText.text = item.itemName;

        if (effectText != null)
            effectText.text = item.description;

        if (quantityText != null)
            quantityText.text = quantity.ToString();

        // Handle optional durability
        if (durabilityText != null)
        {
            if (item.isDurable)
            {
                durabilityText.gameObject.SetActive(true);
                durabilityText.text = $"Durability: {item.maxDurability}";
            }
            else
            {
                durabilityText.gameObject.SetActive(false);
            }
        }

        // Locate the use handler in scene
        useHandler = Object.FindFirstObjectByType<InventoryUseHandler>();

        // Handle Use Button (skip if missing or not applicable)
        if (useButton != null)
        {
            // If this item is from the "Misc" category, hide the use button
            if (item.category == "Misc")
            {
                useButton.gameObject.SetActive(false);
            }
            else
            {
                useButton.gameObject.SetActive(true);
                useButton.onClick.RemoveAllListeners();
                useButton.onClick.AddListener(OnUseButtonClicked);
            }
        }
    }

    /// <summary>
    /// Called when the Use button is clicked.
    /// </summary>
    private void OnUseButtonClicked()
    {
        if (useHandler != null && currentItem != null)
        {
            useHandler.UseItem(currentItem);
        }
        else
        {
            Debug.LogWarning("InventoryItemUI: UseHandler or currentItem is null!");
        }
    }

    /// <summary>
    /// Updates the quantity display when inventory changes.
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        currentQuantity = newQuantity;
        if (quantityText != null)
            quantityText.text = newQuantity.ToString();
    }
}
