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

    [Header("Durability Visuals")]
    public Image fillImage;                     // Fill Area > Fill
    [SerializeField] private Image backgroundImage;   // Slider > Background (now visible in Inspector)

    private InventoryItemData currentItem;
    private int currentQuantity;
    private InventoryUseHandler useHandler;

    public void Setup(InventoryItemData item, int quantity)
    {
        currentItem = item;
        currentQuantity = quantity;

        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (nameText != null)
            nameText.text = item.itemName;

        if (effectText != null)
            effectText.text = item.description;

        if (quantityText != null)
            quantityText.text = quantity.ToString();

        // Handle durability
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

        // Apply rarity color to fill and darker version to background
        if (fillImage != null && backgroundImage != null)
        {
            Color rarityColor = GetRarityColor(item.rarity);
            fillImage.color = rarityColor;
            backgroundImage.color = DarkenColor(rarityColor, 0.5f);
        }

        // Setup Use Button
        useHandler = Object.FindFirstObjectByType<InventoryUseHandler>();

        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();

            if (item.category == "Misc")
            {
                useButton.gameObject.SetActive(false);
            }
            else
            {
                useButton.gameObject.SetActive(true);
                useButton.onClick.AddListener(OnUseButtonClicked);
            }
        }
    }

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

    public void UpdateQuantity(int newQuantity)
    {
        currentQuantity = newQuantity;
        if (quantityText != null)
            quantityText.text = newQuantity.ToString();
    }

    /// <summary>
    /// Returns the color that matches the item's rarity.
    /// </summary>
    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return new Color(0.8f, 0.8f, 0.8f);       // Light Gray
            case ItemRarity.Uncommon: return new Color(0.2f, 0.8f, 0.2f);    // Green
            case ItemRarity.Rare: return new Color(0.2f, 0.4f, 0.8f);        // Blue
            case ItemRarity.Epic: return new Color(0.6f, 0.2f, 0.8f);        // Purple
            case ItemRarity.Legendary: return new Color(0.9f, 0.6f, 0.1f);   // Gold/Orange
            default: return Color.white;
        }
    }

    /// <summary>
    /// Returns a darker version of the given color.
    /// </summary>
    private Color DarkenColor(Color color, float amount = 0.5f)
    {
        return new Color(
            color.r * amount,
            color.g * amount,
            color.b * amount,
            color.a
        );
    }
}
