using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public class InventoryItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID;             // Unique ID for saving/loading
    public string itemName;           // Display name
    public Sprite icon;               // UI icon sprite
    public string category;           // e.g., "Weapon", "Tool", "Food", "Misc"
    [TextArea]
    public string effectDescription;  // e.g., "+25 Hunger, +10 Thirst"

    [Header("Effects")]
    public int restoreHunger;         // Amount to restore hunger (0 if none)
    public int restoreThirst;         // Amount to restore thirst (0 if none)
}
