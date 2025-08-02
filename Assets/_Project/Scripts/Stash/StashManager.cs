using System.Collections.Generic;
using UnityEngine;

public class StashManager : MonoBehaviour
{
    public static StashManager Instance;

    public Dictionary<InventoryItemData, int> stashItems = new();
    public List<ItemInstance> stashInstances = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ItemDatabase.Initialize();

        var (loadedItems, loadedDurables) = SaveManager.LoadStash();
        stashItems = loadedItems ?? new Dictionary<InventoryItemData, int>();
        stashInstances = loadedDurables ?? new List<ItemInstance>();
    }

    public void AddItemToStash(InventoryItemData item, int quantity)
    {
        if (stashItems.ContainsKey(item))
            stashItems[item] += quantity;
        else
            stashItems[item] = quantity;

        SaveManager.SaveStash(stashItems, stashInstances);
    }

    public bool RemoveItemFromStash(InventoryItemData item, int quantity)
    {
        if (stashItems.ContainsKey(item))
        {
            stashItems[item] -= quantity;
            if (stashItems[item] <= 0)
                stashItems.Remove(item);

            SaveManager.SaveStash(stashItems, stashInstances);
            return true;
        }
        return false;
    }

    public void AddInstanceToStash(ItemInstance instance)
    {
        if (!stashInstances.Contains(instance))
        {
            stashInstances.Add(instance);
            SaveManager.SaveStash(stashItems, stashInstances);
        }
    }

    public bool RemoveInstanceFromStash(ItemInstance instance)
    {
        if (stashInstances.Contains(instance))
        {
            stashInstances.Remove(instance);
            SaveManager.SaveStash(stashItems, stashInstances);
            return true;
        }
        return false;
    }

    public bool RemoveStackableItem(InventoryItemData item, int quantity)
    {
        return RemoveItemFromStash(item, quantity);
    }

    public bool RemoveDurableItem(ItemInstance instance)
    {
        return RemoveInstanceFromStash(instance);
    }

    public Dictionary<InventoryItemData, int> GetAllStackables() => stashItems;
    public List<ItemInstance> GetAllInstances() => stashInstances;

    public (Dictionary<InventoryItemData, int>, List<ItemInstance>) GetAllItems()
        => (stashItems, stashInstances);

    public void AddItems(List<InventoryEntry> stackables, List<ItemInstance> durables)
    {
        foreach (var entry in stackables)
            AddItemToStash(entry.itemData, entry.quantity);

        foreach (var inst in durables)
            AddInstanceToStash(inst);

        SaveManager.SaveStash(stashItems, stashInstances);
    }

    public void ClearStash()
    {
        stashItems.Clear();
        stashInstances.Clear();
        SaveManager.SaveStash(stashItems, stashInstances);
    }
}
