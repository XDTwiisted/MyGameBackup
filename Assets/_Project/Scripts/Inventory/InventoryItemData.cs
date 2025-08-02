using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public class InventoryItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID;             // Unique ID for saving/loading
    public string itemName;           // Display name
    public Sprite icon;               // UI icon sprite

    [Tooltip("Category: Food, Tool, Weapon, Health, Misc")]
    public string category;           // Used to filter inventory tabs

    [Tooltip("Simple display type (e.g. Food, Thirst, 9mm, Component)")]
    public string typeLabel;          // Used for inventory UI display only

    [TextArea]
    public string description;        // Can be used for effects, crafting info, etc.

    [Header("Stats for Food / Thirst / Health")]
    public int restoreHunger;
    public int restoreThirst;
    public int restoreHealth;

    [Header("Stats for Weapons")]
    public int damage;
    public string ammoType;

    [Header("Stats for Tools")]
    public string toolModifier;

    [Header("Durability Settings")]
    public int maxDurability;
    public bool isDurable;

    [Header("Modifiers and Effects")]
    [Tooltip("Positive bonus effect shown in green, e.g., '+5% Loot'")]
    public string positiveEffect;

    [Tooltip("Negative effect shown in red, e.g., '-10% Speed'")]
    public string negativeEffect;

    [Tooltip("Tool-specific loot bonus, e.g., 0.05 for +5% loot")]
    [Range(-1f, 1f)]
    public float lootModifier;

    [Header("Loot Info")]
    public ItemRarity rarity;

    [Header("Loot Settings")]
    [Range(0f, 1f)]
    public float dropChance = 0.1f;
    public int minQuantity = 1;
    public int maxQuantity = 1;

    public string GetSimpleTypeLabel()
    {
        return string.IsNullOrEmpty(typeLabel) ? "" : typeLabel;
    }
}
