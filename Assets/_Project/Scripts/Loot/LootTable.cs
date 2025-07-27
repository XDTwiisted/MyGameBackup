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

    [Header("Rarity Drop Chances (Should total 100%)")]
    public List<RarityDropChance> rarityChances = new List<RarityDropChance>
    {
        new RarityDropChance { rarity = ItemRarity.Common, dropChance = 70f },
        new RarityDropChance { rarity = ItemRarity.Uncommon, dropChance = 15f },
        new RarityDropChance { rarity = ItemRarity.Rare, dropChance = 10f },
        new RarityDropChance { rarity = ItemRarity.Epic, dropChance = 4f },
        new RarityDropChance { rarity = ItemRarity.Legendary, dropChance = 1f }
    };

    private Dictionary<ItemRarity, List<LootItem>> lootByRarity = new Dictionary<ItemRarity, List<LootItem>>();

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
            {
                lootByRarity[item.rarity] = new List<LootItem>();
            }

            lootByRarity[item.rarity].Add(new LootItem(item, item.minQuantity, item.maxQuantity, item.dropChance));
        }

        Debug.Log($"[LootTable] Loaded {allLoot.Length} items from Resources/Loot.");
    }

    public List<LootItem> GetLoot()
    {
        List<LootItem> drops = new List<LootItem>();

        ItemRarity selectedRarity = RollForRarity();

        if (lootByRarity.TryGetValue(selectedRarity, out var items) && items.Count > 0)
        {
            LootItem item = items[Random.Range(0, items.Count)];
            drops.Add(item);
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

        return ItemRarity.Common; // fallback
    }
}
