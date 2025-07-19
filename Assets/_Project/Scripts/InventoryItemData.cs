using UnityEngine;

[System.Serializable]
public class InventoryItemData
{
    public string itemID;             // Unique ID for the item
    public string itemName;           // Display name
    public Sprite icon;               // UI icon sprite
    public string category;           // e.g., "Weapon", "Tool", "Food", "Misc"
    public string effectDescription;  // e.g., "+25 Hunger, +10 Thirst"
    public int quantity;              // How many the player has

    // Stat effects — multiple can apply simultaneously
    public int restoreHunger;         // Amount to restore hunger (0 if none)
    public int restoreThirst;         // Amount to restore thirst (0 if none)
}
