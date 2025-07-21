using System.Collections.Generic;
using UnityEngine;

public class LootTable : MonoBehaviour
{
    [Tooltip("Assign possible loot items here with their drop chances and quantities")]
    public List<LootItem> possibleLoot = new List<LootItem>();

    // Tracks newly found item names since last check
    private List<string> newlyFoundItems = new List<string>();

    /// <summary>
    /// Call this method to roll the loot drops.
    /// Returns a list of dropped LootItem with fixed quantities.
    /// Also records found items internally for dialogue display.
    /// </summary>
    public List<LootItem> GetLoot()
    {
        List<LootItem> droppedLoot = new List<LootItem>();

        foreach (LootItem lootItem in possibleLoot)
        {
            float roll = Random.value;
            Debug.Log($"Rolling loot: {lootItem.itemData?.itemName}, chance: {lootItem.dropChance}, roll: {roll}");

            if (roll <= lootItem.dropChance)
            {
                int quantity = Random.Range(lootItem.minQuantity, lootItem.maxQuantity + 1);
                droppedLoot.Add(new LootItem(lootItem.itemData, quantity, quantity, 1f));
                Debug.Log($"Loot dropped: {lootItem.itemData?.itemName} x{quantity}");

                if (lootItem.itemData != null)
                {
                    newlyFoundItems.Add(lootItem.itemData.itemName);
                }
            }
        }

        if (droppedLoot.Count == 0)
        {
            Debug.Log("No loot dropped this roll.");
        }

        return droppedLoot;
    }

    /// <summary>
    /// Returns the list of newly found item names since last call and clears the list.
    /// DialogueManager calls this to show found item messages.
    /// </summary>
    public List<string> GetNewlyFoundItems()
    {
        List<string> items = new List<string>(newlyFoundItems);
        newlyFoundItems.Clear();
        return items;
    }
}
