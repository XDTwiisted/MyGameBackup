using System.Collections.Generic;
using UnityEngine;

public class LootTable : MonoBehaviour
{
    [System.Serializable]
    public class RarityDropChance
    {
        public ItemRarity rarity;
        [Range(0f, 100f)] public float dropChance;
    }

    [Header("Step 1: Global Loot Roll")]
    [Range(0f, 1f)] public float lootChance = 0.75f;

    [Header("Step 2: Rarity Drop Chances (must total ~100%)")]
    public List<RarityDropChance> rarityChances = new List<RarityDropChance>
    {
        new RarityDropChance { rarity = ItemRarity.Common, dropChance = 70f },
        new RarityDropChance { rarity = ItemRarity.Uncommon, dropChance = 15f },
        new RarityDropChance { rarity = ItemRarity.Rare, dropChance = 10f },
        new RarityDropChance { rarity = ItemRarity.Epic, dropChance = 4f },
        new RarityDropChance { rarity = ItemRarity.Legendary, dropChance = 1f }
    };

    private Dictionary<ItemRarity, List<InventoryItemData>> lootByRarity = new();

    private void Awake()
    {
        LoadLootFromResources();
    }

    private void LoadLootFromResources()
    {
        lootByRarity.Clear();
        InventoryItemData[] allLoot = Resources.LoadAll<InventoryItemData>("Loot");

        foreach (var item in allLoot)
        {
            if (item == null) continue;

            if (!lootByRarity.ContainsKey(item.rarity))
                lootByRarity[item.rarity] = new List<InventoryItemData>();

            lootByRarity[item.rarity].Add(item);
        }

        Debug.Log($"[LootTable] Loaded {allLoot.Length} items from Resources/Loot.");
    }

    public List<ItemInstance> GetLoot()
    {
        List<ItemInstance> drops = new();

        // Step 1: Global loot chance
        float globalRoll = Random.Range(0f, 1f);
        if (globalRoll > lootChance)
        {
            Debug.Log("[LootTable] No loot (global chance roll failed)");
            return drops;
        }

        // Step 2: Roll for rarity
        ItemRarity selectedRarity = RollForRarity();
        Debug.Log($"[LootTable] Rolled rarity: {selectedRarity}");

        if (!lootByRarity.TryGetValue(selectedRarity, out var items) || items.Count == 0)
        {
            Debug.LogWarning($"[LootTable] No items found for rarity {selectedRarity}");
            return drops;
        }

        // Step 3: Weighted selection based on dropChance
        InventoryItemData selectedItem = SelectWeightedItem(items);

        if (selectedItem == null)
        {
            Debug.LogWarning("[LootTable] No item passed dropChance check");
            return drops;
        }

        int quantity = Random.Range(selectedItem.minQuantity, selectedItem.maxQuantity + 1);

        if (selectedItem.isDurable)
        {
            for (int i = 0; i < quantity; i++)
            {
                int durability = Random.Range(1, selectedItem.maxDurability + 1);
                drops.Add(new ItemInstance(selectedItem, 1, durability));
                Debug.Log($"[LootTable] Dropped durable: {selectedItem.itemName} (Durability: {durability})");
            }
        }
        else
        {
            drops.Add(new ItemInstance(selectedItem, quantity, 0));
            Debug.Log($"[LootTable] Dropped stackable: {selectedItem.itemName} x{quantity}");
        }

        return drops;
    }

    private ItemRarity RollForRarity()
    {
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        foreach (var rarity in rarityChances)
        {
            cumulative += rarity.dropChance;
            if (roll <= cumulative)
                return rarity.rarity;
        }

        return ItemRarity.Common;
    }

    private InventoryItemData SelectWeightedItem(List<InventoryItemData> items)
    {
        float totalWeight = 0f;
        foreach (var item in items)
        {
            totalWeight += item.dropChance;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var item in items)
        {
            cumulative += item.dropChance;
            if (roll <= cumulative)
                return item;
        }

        return null;
    }
}
