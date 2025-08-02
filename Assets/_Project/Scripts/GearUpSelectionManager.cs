using System.Collections.Generic;
using UnityEngine;

public class GearUpSelectionManager : MonoBehaviour
{
    public static GearUpSelectionManager Instance;

    private Dictionary<InventoryItemData, int> selectedStackables = new Dictionary<InventoryItemData, int>();
    private List<ItemInstance> selectedDurables = new List<ItemInstance>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Call this when player selects stackable items like food, water, health
    public void AddStackable(InventoryItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0) return;

        if (selectedStackables.ContainsKey(itemData))
            selectedStackables[itemData] += quantity;
        else
            selectedStackables[itemData] = quantity;
    }

    // Call this when player selects tools/weapons
    public void AddDurable(ItemInstance instance)
    {
        if (instance != null && instance.itemData != null)
            selectedDurables.Add(instance);
    }

    // These will be called when player clicks "Explore"
    public Dictionary<InventoryItemData, int> GetStackables()
    {
        return selectedStackables;
    }

    public List<ItemInstance> GetDurables()
    {
        return selectedDurables;
    }

    // Clear selection after player starts exploring
    public void ClearSelections()
    {
        selectedStackables.Clear();
        selectedDurables.Clear();
    }
}
