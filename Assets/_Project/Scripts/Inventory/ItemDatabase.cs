using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    private static Dictionary<string, InventoryItemData> itemDict;

    public static void Initialize(List<InventoryItemData> allItems)
    {
        itemDict = new Dictionary<string, InventoryItemData>();
        foreach (var item in allItems)
        {
            if (!itemDict.ContainsKey(item.itemID))
                itemDict.Add(item.itemID, item);
        }
    }

    public static InventoryItemData FindItemByID(string id)
    {
        if (itemDict != null && itemDict.TryGetValue(id, out var item))
            return item;
        return null;
    }
}
