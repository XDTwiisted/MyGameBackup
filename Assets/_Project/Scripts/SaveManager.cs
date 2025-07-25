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
    }

    public static List<InventoryEntry> LoadInventory()
    {
        List<InventoryEntry> loadedInventory = new List<InventoryEntry>();

        if (PlayerPrefs.HasKey("inventory"))
        {
            string json = PlayerPrefs.GetString("inventory");
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            foreach (var saveItem in saveData.items)
            {
                InventoryItemData item = ItemDatabase.FindItemByID(saveItem.itemID);
                if (item != null)
                {
                    loadedInventory.Add(new InventoryEntry(item, saveItem.count));
                }
            }
        }

        return loadedInventory;
    }
}
