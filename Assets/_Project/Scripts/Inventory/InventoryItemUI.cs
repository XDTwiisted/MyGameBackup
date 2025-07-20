using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI effectText;
    public TextMeshProUGUI quantityText;
    public Button useButton;

    private InventoryItemData currentItem;
    private int currentQuantity;
    private InventoryUseHandler useHandler;

    public void Setup(InventoryItemData item, int quantity)
    {
        currentItem = item;
        currentQuantity = quantity;

        if (iconImage != null) iconImage.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;
        if (effectText != null) effectText.text = item.effectDescription;
        if (quantityText != null) quantityText.text = quantity.ToString();

        // Updated line to avoid deprecation warning
        useHandler = Object.FindFirstObjectByType<InventoryUseHandler>();

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

    public void UpdateQuantity(int newQuantity)
    {
        currentQuantity = newQuantity;
        if (quantityText != null)
            quantityText.text = newQuantity.ToString();
    }
}
