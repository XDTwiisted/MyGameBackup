using System.Collections.Generic;
using UnityEngine;

public class LootTable : MonoBehaviour
{
    [System.Serializable]
    public class RarityChance
    {
        public ItemRarity rarity;
        [Range(0f, 100f)] public float chance;
    }

    [Tooltip("Loot rarity distribution. Should total 100%")]
    public List<RarityChance> rarityChances;

    private List<LootItem> possibleLoot = new List<LootItem>();
    private List<string> newlyFoundItems = new List<string>();

    // Timer to control when to attempt a loot drop
    public float lootDropInterval = 15f; // base interval in seconds
    private float lootDropTimer = 0f;

    private void Awake()
    {
        LoadLootFromResources();
    }

    private void LoadLootFromResources()
    {
        possibleLoot.Clear();

        InventoryItemData[] allItems = Resources.LoadAll<InventoryItemData>("Loot");

        foreach (var item in allItems)
        {
            if (item != null)
            {
                possibleLoot.Add(new LootItem(item, item.minQuantity, item.maxQuantity, item.dropChance));
            }
        }

        Debug.Log($"[LootTable] Loaded {possibleLoot.Count} loot items from Resources/Loot/");
    }

    // Call this in Update with scaled deltaTime to speed up loot drops
    public void UpdateLoot(float deltaTime)
    {
        lootDropTimer += deltaTime;

        if (lootDropTimer >= lootDropInterval)
        {
            lootDropTimer = 0f;
            List<LootItem> dropped = GetLoot();
            // Optionally handle the dropped loot here or notify other systems
        }
    }

    public List<LootItem> GetLoot()
    {
        List<LootItem> droppedLoot = new List<LootItem>();

        int numberOfRolls = 1;

        for (int i = 0; i < numberOfRolls; i++)
        {
            ItemRarity selectedRarity = RollRarity();

            List<LootItem> filteredLoot = possibleLoot.FindAll(loot =>
                loot.itemData != null && loot.itemData.rarity == selectedRarity);

            if (filteredLoot.Count == 0)
            {
                Debug.LogWarning($"No loot items found for rarity: {selectedRarity}");
                continue;
            }

            LootItem chosenLoot = filteredLoot[Random.Range(0, filteredLoot.Count)];
            float roll = Random.value;

            float dropChance = chosenLoot.itemData.dropChance;

            Debug.Log($"Roll #{i + 1}: Rarity={selectedRarity}, Item={chosenLoot.itemData.itemName}, DropChance={dropChance}, Roll={roll}");

            if (roll <= dropChance)
            {
                int quantity = Random.Range(chosenLoot.itemData.minQuantity, chosenLoot.itemData.maxQuantity + 1);
                droppedLoot.Add(new LootItem(chosenLoot.itemData, quantity, quantity, 1f));
                newlyFoundItems.Add(chosenLoot.itemData.itemName);

                Debug.Log($"Dropped: {chosenLoot.itemData.itemName} x{quantity}");
            }
        }

        if (droppedLoot.Count == 0)
        {
            Debug.Log("No loot dropped this roll.");
        }

        return droppedLoot;
    }

    private ItemRarity RollRarity()
    {
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        foreach (var rc in rarityChances)
        {
            cumulative += rc.chance;
            if (roll <= cumulative)
                return rc.rarity;
        }

        Debug.LogWarning("Rarity roll failed, defaulting to Common");
        return ItemRarity.Common;
    }

    public List<string> GetNewlyFoundItems()
    {
        List<string> items = new List<string>(newlyFoundItems);
        newlyFoundItems.Clear();
        return items;
    }
}
