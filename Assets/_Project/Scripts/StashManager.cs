using System.Collections.Generic;
using UnityEngine;

public class StashManager : MonoBehaviour
{
    public static StashManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        //  Initialize item database BEFORE loading stash
        ItemDatabase.Initialize();

        //  Load stash from saved data
        var (loadedItems, loadedDurables) = SaveManager.LoadStash();
        stashItems = loadedItems ?? new Dictionary<InventoryItemData, int>();
        stashInstances = loadedDurables ?? new List<ItemInstance>();

        Debug.Log($"Stash loaded: {stashItems.Count} stackables, {stashInstances.Count} durables");
    }

    // Stackables
    public Dictionary<InventoryItemData, int> stashItems = new Dictionary<InventoryItemData, int>();

    // Durables
    public List<ItemInstance> stashInstances = new List<ItemInstance>();

    public void AddItemToStash(InventoryItemData item, int quantity)
    {
        if (stashItems.ContainsKey(item))
            stashItems[item] += quantity;
        else
            stashItems[item] = quantity;

        SaveManager.SaveStash(stashItems, stashInstances);
    }

    public void RemoveItemFromStash(InventoryItemData item, int quantity)
    {
        if (stashItems.ContainsKey(item))
        {
            stashItems[item] -= quantity;
            if (stashItems[item] <= 0)
                stashItems.Remove(item);

            SaveManager.SaveStash(stashItems, stashInstances);
        }
    }

    public void AddInstanceToStash(ItemInstance instance)
    {
        if (!stashInstances.Contains(instance))
        {
            stashInstances.Add(instance);
            SaveManager.SaveStash(stashItems, stashInstances);
        }
    }

    public void RemoveInstanceFromStash(ItemInstance instance)
    {
        if (stashInstances.Contains(instance))
        {
            stashInstances.Remove(instance);
            SaveManager.SaveStash(stashItems, stashInstances);
        }
    }

    public Dictionary<InventoryItemData, int> GetAllStackables()
    {
        return stashItems;
    }

    public List<ItemInstance> GetAllInstances()
    {
        return stashInstances;
    }

    public (Dictionary<InventoryItemData, int>, List<ItemInstance>) GetAllItems()
    {
        return (stashItems, stashInstances);
    }

    public void AddItems(List<InventoryEntry> stackables, List<ItemInstance> durables)
    {
        foreach (var entry in stackables)
        {
            AddItemToStash(entry.itemData, entry.quantity);
        }

        foreach (var instance in durables)
        {
            AddInstanceToStash(instance);
        }

        Debug.Log($"Added {stackables.Count} stackable and {durables.Count} durable items to stash.");
        SaveManager.SaveStash(stashItems, stashInstances);
    }

    public void ClearStash()
    {
        stashItems.Clear();
        stashInstances.Clear();
        SaveManager.SaveStash(stashItems, stashInstances);
    }
}
