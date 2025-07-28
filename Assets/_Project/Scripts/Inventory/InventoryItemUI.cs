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
    public TextMeshProUGUI durabilityText;
    public Button useButton;

    [Header("Durability Visuals")]
    public Slider durabilitySlider;
    public Image fillImage;
    [SerializeField] private Image backgroundImage;

    private InventoryItemData currentItem;
    private int currentQuantity;
    private int currentDurability;
    private InventoryUseHandler useHandler;

    public void Setup(InventoryItemData item, int quantity, int durability = -1)
    {
        currentItem = item;
        currentQuantity = quantity;
        currentDurability = durability;

        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (nameText != null)
            nameText.text = item.itemName;

        if (effectText != null)
            effectText.text = item.description;

        if (quantityText != null)
            quantityText.text = quantity.ToString();

        if (durabilityText != null)
        {
            if (item.isDurable)
            {
                durabilityText.gameObject.SetActive(true);
                durabilityText.text = $"Durability: {currentDurability}/{item.maxDurability}";
            }
            else
            {
                durabilityText.gameObject.SetActive(false);
            }
        }

        if (item.isDurable && durabilitySlider != null)
        {
            durabilitySlider.gameObject.SetActive(true);
            durabilitySlider.maxValue = item.maxDurability;
            durabilitySlider.value = currentDurability;
        }
        else if (durabilitySlider != null)
        {
            durabilitySlider.gameObject.SetActive(false);
        }

        if (fillImage != null && backgroundImage != null)
        {
            Color rarityColor = GetRarityColor(item.rarity);
            fillImage.color = rarityColor;
            backgroundImage.color = DarkenColor(rarityColor, 0.5f);
        }

        useHandler = UnityEngine.Object.FindFirstObjectByType<InventoryUseHandler>();

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

    public void SetItem(InventoryItemData item, int quantity)
    {
        Setup(item, quantity);
    }

    public void SetItemInstance(ItemInstance instance)
    {
        if (instance == null || instance.itemData == null)
        {
            Debug.LogWarning("SetItemInstance: instance or itemData is null.");
            return;
        }

        Setup(instance.itemData, instance.quantity, instance.currentDurability);
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

    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return new Color(0.8f, 0.8f, 0.8f);
            case ItemRarity.Uncommon: return new Color(0.2f, 0.8f, 0.2f);
            case ItemRarity.Rare: return new Color(0.2f, 0.4f, 0.8f);
            case ItemRarity.Epic: return new Color(0.6f, 0.2f, 0.8f);
            case ItemRarity.Legendary: return new Color(0.9f, 0.6f, 0.1f);
            default: return Color.white;
        }
    }

    private Color DarkenColor(Color color, float amount = 0.5f)
    {
        return new Color(color.r * amount, color.g * amount, color.b * amount, color.a);
    }
}
