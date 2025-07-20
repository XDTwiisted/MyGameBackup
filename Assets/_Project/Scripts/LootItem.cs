using UnityEngine;

[System.Serializable]
public class LootItem
{
    public InventoryItemData itemData;  // Reference to the full item info
    public int minQuantity;             // Minimum amount that can drop
    public int maxQuantity;             // Maximum amount that can drop
    public float dropChance;            // Probability of drop (0 to 1)

    // Constructor to create LootItem in code
    public LootItem(InventoryItemData data, int minQ, int maxQ, float chance)
    {
        itemData = data;
        minQuantity = minQ;
        maxQuantity = maxQ;
        dropChance = chance;
    }
}
