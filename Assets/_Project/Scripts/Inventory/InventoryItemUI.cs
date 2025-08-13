using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI effectText;

    [Header("Durability Visuals")]
    public Slider durabilitySlider;
    public Image fillImage;
    [SerializeField] private Image backgroundImage;

    [Header("Drop UI")]
    public Button dropButton;
    public GameObject dropConfirmationPanel;
    public Button yesDropButton;
    public Button noDropButton;

    private InventoryItemData currentItem;
    private int currentQuantity;
    private int currentDurability;
    private ItemInstance currentInstance;

    private void Awake()
    {
        if (dropButton != null)
            dropButton.onClick.AddListener(ShowDropConfirmation);

        if (yesDropButton != null)
            yesDropButton.onClick.AddListener(DropItem);

        if (noDropButton != null)
            noDropButton.onClick.AddListener(HideDropConfirmation);

        if (dropConfirmationPanel != null)
            dropConfirmationPanel.SetActive(false);
    }

    public void Setup(InventoryItemData item, int quantity, int durability = -1)
    {
        currentItem = item;
        currentQuantity = quantity;
        currentDurability = durability;
        currentInstance = null;

        ApplyCommonUI(item, quantity, durability);
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

        currentInstance = instance;
        currentItem = instance.itemData;
        currentQuantity = instance.quantity;
        currentDurability = instance.currentDurability;

        ApplyCommonUI(currentItem, currentQuantity, currentDurability);
    }

    private void ApplyCommonUI(InventoryItemData item, int quantity, int durability)
    {
        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (nameText != null)
            nameText.text = item.itemName;

        if (typeText != null)
            typeText.text = GetStatTypeDisplay(item);

        if (damageText != null)
            damageText.text = (item.category == "Weapon" && item.damage > 0) ? $"+{item.damage} Damage" : "";

        if (ammoText != null)
            ammoText.text = (item.category == "Weapon" && !string.IsNullOrEmpty(item.ammoType)) ? item.ammoType : "";

        if (effectText != null)
            effectText.text = GetEffectDisplay(item);

        if (quantityText != null)
            quantityText.text = item.isDurable ? "" : quantity.ToString();

        if (durabilitySlider != null)
        {
            if (item.isDurable)
            {
                durabilitySlider.gameObject.SetActive(true);
                durabilitySlider.maxValue = item.maxDurability;
                durabilitySlider.value = durability;
            }
            else
            {
                durabilitySlider.gameObject.SetActive(false);
            }
        }

        if (fillImage != null && backgroundImage != null)
        {
            Color rarityColor = RarityColors.GetColor(item.rarity);
            fillImage.color = rarityColor;
            backgroundImage.color = DarkenColor(rarityColor, 0.5f);
        }

        HideDropConfirmation();
    }

    private string GetStatTypeDisplay(InventoryItemData item)
    {
        // Always show both Hunger and Thirst for food/thirst items, even when zero
        if (item.category == "Food" || item.category == "Thirst" || item.category == "Drink")
        {
            return $"+{item.restoreHunger} Hunger | +{item.restoreThirst} Thirst";
        }

        // Default behavior for other categories
        string result = "";

        if (item.restoreHunger > 0)
            result += $"+{item.restoreHunger} Hunger";

        if (item.restoreThirst > 0)
            result += (result.Length > 0 ? " | " : "") + $"+{item.restoreThirst} Thirst";

        if (item.restoreHealth > 0)
            result += (result.Length > 0 ? " | " : "") + $"+{item.restoreHealth} Health";

        if (string.IsNullOrEmpty(result))
            result = string.IsNullOrEmpty(item.typeLabel) ? "" : item.typeLabel;

        return result;
    }

    private string GetEffectDisplay(InventoryItemData item)
    {
        string effect = "";

        if (!string.IsNullOrEmpty(item.positiveEffect))
            effect += item.positiveEffect;

        if (!string.IsNullOrEmpty(item.negativeEffect))
        {
            if (!string.IsNullOrEmpty(effect))
                effect += " | ";
            effect += item.negativeEffect;
        }

        return effect;
    }

    private void ShowDropConfirmation()
    {
        if (dropConfirmationPanel != null)
            dropConfirmationPanel.SetActive(true);
    }

    private void HideDropConfirmation()
    {
        if (dropConfirmationPanel != null)
            dropConfirmationPanel.SetActive(false);
    }

    private void DropItem()
    {
        bool dropped = false;

        if (currentItem != null)
        {
            if (currentItem.isDurable && currentInstance != null)
            {
                if (InventoryManager.Instance != null)
                    dropped = InventoryManager.Instance.RemoveDurableItem(currentInstance);

                if (!dropped && StashManager.Instance != null)
                    dropped = StashManager.Instance.RemoveDurableItem(currentInstance);
            }
            else
            {
                if (InventoryManager.Instance != null)
                    dropped = InventoryManager.Instance.RemoveStackableItem(currentItem, 1);

                if (!dropped && StashManager.Instance != null)
                    dropped = StashManager.Instance.RemoveStackableItem(currentItem, 1);
            }

            if (dropped)
                Debug.Log($"Dropped item: {currentItem.itemName}");
            else
                Debug.LogWarning("Drop failed: item not found in inventory or stash");
        }

        InventoryUIManager.Instance?.RefreshInventoryDisplay();
        StashManagerUI.Instance?.RefreshStashUI();
        HideDropConfirmation();
    }

    public void UpdateQuantity(int newQuantity)
    {
        currentQuantity = newQuantity;
        if (quantityText != null)
            quantityText.text = newQuantity.ToString();
    }

    private Color DarkenColor(Color color, float amount = 0.5f)
    {
        return new Color(color.r * amount, color.g * amount, color.b * amount, color.a);
    }
}
