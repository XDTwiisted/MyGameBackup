using System.Collections.Generic;
using UnityEngine;

public class GearUpSelectionManager : MonoBehaviour
{
    public static GearUpSelectionManager Instance;

    // Stackables: aggregated by InventoryItemData reference
    private readonly Dictionary<InventoryItemData, int> selectedStackables = new Dictionary<InventoryItemData, int>();

    // Durables: unique ItemInstance references
    private readonly List<ItemInstance> selectedDurables = new List<ItemInstance>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // Add or increase a stackable selection (food, health, etc.)
    public void AddStackable(InventoryItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return;

        int current;
        if (selectedStackables.TryGetValue(itemData, out current))
            selectedStackables[itemData] = current + quantity;
        else
            selectedStackables[itemData] = quantity;
    }

    // Decrease a stackable selection; remove if reaches zero
    public bool RemoveStackable(InventoryItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return false;

        int current;
        if (!selectedStackables.TryGetValue(itemData, out current)) return false;

        current -= quantity;
        if (current > 0)
        {
            selectedStackables[itemData] = current;
        }
        else
        {
            selectedStackables.Remove(itemData);
        }
        return true;
    }

    // Set exact quantity for a stackable (min 0)
    public void SetStackableQuantity(InventoryItemData itemData, int quantity)
    {
        if (itemData == null) return;
        if (quantity <= 0) selectedStackables.Remove(itemData);
        else selectedStackables[itemData] = quantity;
    }

    public int GetStackableQuantity(InventoryItemData itemData)
    {
        if (itemData == null) return 0;
        int q;
        return selectedStackables.TryGetValue(itemData, out q) ? q : 0;
    }

    public bool HasStackable(InventoryItemData itemData)
    {
        if (itemData == null) return false;
        return selectedStackables.ContainsKey(itemData);
    }

    // Add a durable selection (weapon, tool). Prevent duplicates.
    public void AddDurable(ItemInstance instance)
    {
        if (instance == null || instance.itemData == null) return;
        if (!selectedDurables.Contains(instance))
            selectedDurables.Add(instance);
    }

    // Remove a durable selection (used by swap logic)
    public bool RemoveDurable(ItemInstance instance)
    {
        if (instance == null) return false;
        return selectedDurables.Remove(instance);
    }

    public bool HasDurable(ItemInstance instance)
    {
        if (instance == null) return false;
        return selectedDurables.Contains(instance);
    }

    // Get copies so callers cannot mutate internal collections by accident
    public Dictionary<InventoryItemData, int> GetStackables()
    {
        return new Dictionary<InventoryItemData, int>(selectedStackables);
    }

    public List<ItemInstance> GetDurables()
    {
        return new List<ItemInstance>(selectedDurables);
    }

    // Clear all selections (called after Confirm Exploration handoff)
    public void ClearSelections()
    {
        selectedStackables.Clear();
        selectedDurables.Clear();
    }
}
