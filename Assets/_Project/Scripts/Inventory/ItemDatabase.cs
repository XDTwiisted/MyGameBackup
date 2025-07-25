using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, InventoryItemData> itemLookup = new Dictionary<string, InventoryItemData>();

    public static void Initialize(List<InventoryItemData> predefinedItems = null)
    {
        itemLookup.Clear();

        // Load all InventoryItemData from Resources/Loot/
        InventoryItemData[] allItems = Resources.LoadAll<InventoryItemData>("Loot");

        foreach (var item in allItems)
        {
            if (!itemLookup.ContainsKey(item.itemID))
            {
                itemLookup.Add(item.itemID, item);
            }
            else
            {
                Debug.LogWarning("Duplicate item ID found in Resources: " + item.itemID);
            }
        }

        // Also add predefined ones if any (from InventoryManager)
        if (predefinedItems != null)
        {
            foreach (var item in predefinedItems)
            {
                if (!itemLookup.ContainsKey(item.itemID))
                {
                    itemLookup.Add(item.itemID, item);
                }
            }
        }

        Debug.Log("ItemDatabase initialized with " + itemLookup.Count + " items.");
    }

    public static InventoryItemData FindItemByID(string id)
    {
        InventoryItemData item;
        if (itemLookup.TryGetValue(id, out item))
        {
            return item;
        }

        return null;
    }
}
