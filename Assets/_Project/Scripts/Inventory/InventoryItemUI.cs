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
    public TextMeshProUGUI ammoText; // ADDED
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI effectText;

    [Header("Durability Visuals")]
    public Slider durabilitySlider;
    public Image fillImage;
    [SerializeField] private Image backgroundImage;

    private InventoryItemData currentItem;
    private int currentQuantity;
    private int currentDurability;

    public void Setup(InventoryItemData item, int quantity, int durability = -1)
    {
        currentItem = item;
        currentQuantity = quantity;
        currentDurability = durability;

        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (nameText != null)
            nameText.text = item.itemName;

        if (typeText != null)
            typeText.text = GetStatTypeDisplay(item);

        if (damageText != null)
        {
            if (item.category == "Weapon" && item.damage > 0)
                damageText.text = $"+{item.damage} Damage";
            else
                damageText.text = "";
        }

        if (ammoText != null)
        {
            if (item.category == "Weapon" && !string.IsNullOrEmpty(item.ammoType))
                ammoText.text = item.ammoType;
            else
                ammoText.text = "";
        }


        if (effectText != null)
            effectText.text = GetEffectDisplay(item);

        if (quantityText != null)
        {
            if (item.isDurable)
                quantityText.text = "";
            else
                quantityText.text = quantity.ToString();
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
            Color rarityColor = RarityColors.GetColor(item.rarity);
            fillImage.color = rarityColor;
            backgroundImage.color = DarkenColor(rarityColor, 0.5f);
        }
    }

    private string GetStatTypeDisplay(InventoryItemData item)
    {
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
