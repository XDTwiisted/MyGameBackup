using System.Collections.Generic;
using UnityEngine;

public static class SaveManager
{
    public static void SaveInventory(List<InventoryEntry> inventory)
    {
        InventorySaveData saveData = new InventorySaveData();
        foreach (var entry in inventory)
        {
            saveData.items.Add(new InventoryItemSave
            {
                itemID = entry.itemData.itemID,
                count = entry.quantity
            });
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("inventory", json);
        PlayerPrefs.Save();

        Debug.Log("Inventory saved: " + json);
    }

    public static List<InventoryEntry> LoadInventory()
    {
        List<InventoryEntry> loadedInventory = new List<InventoryEntry>();

        if (PlayerPrefs.HasKey("inventory"))
        {
            string json = PlayerPrefs.GetString("inventory");
            Debug.Log("Inventory loaded from PlayerPrefs: " + json);

            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            foreach (var saveItem in saveData.items)
            {
                InventoryItemData item = ItemDatabase.FindItemByID(saveItem.itemID);
                if (item != null)
                {
                    loadedInventory.Add(new InventoryEntry(item, saveItem.count));
                    Debug.Log($"Loaded item: {item.itemName} x{saveItem.count}");
                }
                else
                {
                    Debug.LogWarning("Could not find InventoryItemData for ID: " + saveItem.itemID);
                }
            }
        }
        else
        {
            Debug.Log("No inventory found in PlayerPrefs.");
        }

        return loadedInventory;
    }
}
