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

    [TextArea]
    public string description;        // Can be used for effects, crafting info, etc.

    [Header("Stats for Food / Thirst / Health")]
    public int restoreHunger;         // Restores hunger (optional)
    public int restoreThirst;         // Restores thirst (optional)
    public int restoreHealth;         // Restores health (optional)

    [Header("Stats for Tools / Weapons")]
    public int maxDurability;         // Optional: tools/weapons
    public bool isDurable;            // True if item uses durability

    [Header("Loot Info")]
    public ItemRarity rarity;         // Rarity tier for loot table

    [Header("Loot Settings")]
    [Range(0f, 1f)]
    public float dropChance = 0.1f;  // Chance to drop this item (0 to 1)

    public int minQuantity = 1;      // Minimum quantity dropped
    public int maxQuantity = 1;      // Maximum quantity dropped
}
