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

    public ItemInstanceSave() { }
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
    private const string StashInventoryKey = "stashInventory";
    private const string StashRuntimeKey = "stashRuntime";

    // ---------- INVENTORY ----------

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

    public static List<InventoryEntry> LoadInventory()
    {
        List<InventoryEntry> loadedInventory = new List<InventoryEntry>();

        if (PlayerPrefs.HasKey(InventoryKey))
        {
            string json = PlayerPrefs.GetString(InventoryKey);
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            foreach (var item in saveData.items)
            {
                var itemData = ItemDatabase.FindItemByID(item.itemID);
                if (itemData != null)
                {
                    loadedInventory.Add(new InventoryEntry(itemData, item.count));
                    Debug.Log($"Loaded inventory item: {itemData.itemName} x{item.count}");
                }
                else
                {
                    Debug.LogWarning("Missing InventoryItemData for: " + item.itemID);
                }
            }
        }

        return loadedInventory;
    }

    public static void SaveRuntimeInventory(List<ItemInstance> runtimeInventory)
    {
        RuntimeInventorySaveData saveData = new RuntimeInventorySaveData();
        foreach (var instance in runtimeInventory)
        {
            saveData.items.Add(new ItemInstanceSave(instance.itemData.itemID, instance.quantity, instance.currentDurability));
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(RuntimeInventoryKey, json);
        PlayerPrefs.Save();
        Debug.Log("Runtime inventory saved: " + json);
    }

    public static List<ItemInstance> LoadRuntimeInventory()
    {
        List<ItemInstance> loaded = new List<ItemInstance>();

        if (PlayerPrefs.HasKey(RuntimeInventoryKey))
        {
            string json = PlayerPrefs.GetString(RuntimeInventoryKey);
            RuntimeInventorySaveData saveData = JsonUtility.FromJson<RuntimeInventorySaveData>(json);

            foreach (var item in saveData.items)
            {
                var itemData = ItemDatabase.FindItemByID(item.itemID);
                if (itemData != null)
                {
                    loaded.Add(new ItemInstance(itemData, item.quantity, item.currentDurability));
                    Debug.Log($"Loaded runtime item: {itemData.itemName} x{item.quantity} (Durability: {item.currentDurability})");
                }
                else
                {
                    Debug.LogWarning("Missing ItemData for runtime item: " + item.itemID);
                }
            }
        }

        return loaded;
    }

    // ---------- STASH ----------

    public static void SaveStash(Dictionary<InventoryItemData, int> stashItems, List<ItemInstance> stashInstances)
    {
        InventorySaveData stashData = new InventorySaveData();
        foreach (var kvp in stashItems)
        {
            stashData.items.Add(new InventoryItemSave
            {
                itemID = kvp.Key.itemID,
                count = kvp.Value
            });
        }

        PlayerPrefs.SetString(StashInventoryKey, JsonUtility.ToJson(stashData));

        RuntimeInventorySaveData stashRuntimeData = new RuntimeInventorySaveData();
        foreach (var instance in stashInstances)
        {
            stashRuntimeData.items.Add(new ItemInstanceSave(instance.itemData.itemID, instance.quantity, instance.currentDurability));
        }

        PlayerPrefs.SetString(StashRuntimeKey, JsonUtility.ToJson(stashRuntimeData));
        PlayerPrefs.Save();

        Debug.Log("Stash saved.");
    }

    public static (Dictionary<InventoryItemData, int>, List<ItemInstance>) LoadStash()
    {
        Dictionary<InventoryItemData, int> loadedStashItems = new Dictionary<InventoryItemData, int>();
        List<ItemInstance> loadedStashInstances = new List<ItemInstance>();

        if (PlayerPrefs.HasKey(StashInventoryKey))
        {
            string stashJson = PlayerPrefs.GetString(StashInventoryKey);
            InventorySaveData stashData = JsonUtility.FromJson<InventorySaveData>(stashJson);

            foreach (var entry in stashData.items)
            {
                var itemData = ItemDatabase.FindItemByID(entry.itemID);
                if (itemData != null)
                {
                    loadedStashItems[itemData] = entry.count;
                    Debug.Log($"Loaded stash stackable: {itemData.itemName} x{entry.count}");
                }
                else
                {
                    Debug.LogWarning("Missing item data for stash stackable: " + entry.itemID);
                }
            }
        }

        if (PlayerPrefs.HasKey(StashRuntimeKey))
        {
            string runtimeJson = PlayerPrefs.GetString(StashRuntimeKey);
            RuntimeInventorySaveData runtimeData = JsonUtility.FromJson<RuntimeInventorySaveData>(runtimeJson);

            foreach (var saved in runtimeData.items)
            {
                var itemData = ItemDatabase.FindItemByID(saved.itemID);
                if (itemData != null)
                {
                    loadedStashInstances.Add(new ItemInstance(itemData, saved.quantity, saved.currentDurability));
                    Debug.Log($"Loaded stash durable: {itemData.itemName} x{saved.quantity} (Durability: {saved.currentDurability})");
                }
                else
                {
                    Debug.LogWarning("Missing item data for stash durable: " + saved.itemID);
                }
            }
        }

        return (loadedStashItems, loadedStashInstances);
    }
}
