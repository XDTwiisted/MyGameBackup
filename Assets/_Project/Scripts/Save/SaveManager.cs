using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemInstanceSave
{
    public string itemID;
    public int quantity;
    public int currentDurability;

    public ItemInstanceSave(string id, int qty, int durability)
    {
        itemID = id;
        quantity = qty;
        currentDurability = durability;
    }
}

[System.Serializable]
public class RuntimeInventorySaveData
{
    public List<ItemInstanceSave> items = new List<ItemInstanceSave>();
}

public static class SaveManager
{
    private const string InventoryKey = "inventory";
    private const string RuntimeInventoryKey = "runtimeInventory";

    // Save non-durable stackable items (e.g., food, meds)
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
        PlayerPrefs.SetString(InventoryKey, json);
        PlayerPrefs.Save();

        Debug.Log("Inventory saved: " + json);
    }

    // Load stackable items
    public static List<InventoryEntry> LoadInventory()
    {
        List<InventoryEntry> loadedInventory = new List<InventoryEntry>();

        if (PlayerPrefs.HasKey(InventoryKey))
        {
            string json = PlayerPrefs.GetString(InventoryKey);
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

    // Save durable runtime-only items (e.g., weapons, tools)
    public static void SaveRuntimeInventory(List<ItemInstance> runtimeInventory)
    {
        List<ItemInstanceSave> saveList = new List<ItemInstanceSave>();

        foreach (var item in runtimeInventory)
        {
            saveList.Add(new ItemInstanceSave(item.itemData.itemID, item.quantity, item.currentDurability));
        }

        string json = JsonUtility.ToJson(new RuntimeInventorySaveData { items = saveList });
        PlayerPrefs.SetString(RuntimeInventoryKey, json);
        PlayerPrefs.Save();

        Debug.Log("Runtime inventory saved: " + json);
    }

    // Load durable runtime-only items
    public static List<ItemInstance> LoadRuntimeInventory()
    {
        List<ItemInstance> loaded = new List<ItemInstance>();

        if (!PlayerPrefs.HasKey(RuntimeInventoryKey))
        {
            Debug.Log("No runtime inventory found.");
            return loaded;
        }

        string json = PlayerPrefs.GetString(RuntimeInventoryKey);
        Debug.Log("Runtime inventory loaded: " + json);

        RuntimeInventorySaveData saveData = JsonUtility.FromJson<RuntimeInventorySaveData>(json);

        foreach (var saved in saveData.items)
        {
            InventoryItemData itemData = ItemDatabase.FindItemByID(saved.itemID);
            if (itemData != null)
            {
                loaded.Add(new ItemInstance(itemData, saved.quantity, saved.currentDurability));
                Debug.Log($"Loaded durable item: {itemData.itemName} (Durability: {saved.currentDurability})");
            }
            else
            {
                Debug.LogWarning("Could not find InventoryItemData for runtime ID: " + saved.itemID);
            }
        }

        return loaded;
    }
}
