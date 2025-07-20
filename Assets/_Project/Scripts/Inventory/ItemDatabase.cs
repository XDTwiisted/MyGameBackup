using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<InventoryItemData> allItems = new List<InventoryItemData>();

    private Dictionary<string, InventoryItemData> itemLookup;

    public void BuildLookup()
    {
        itemLookup = new Dictionary<string, InventoryItemData>();
        foreach (var item in allItems)
        {
            if (!itemLookup.ContainsKey(item.itemName))
            {
                itemLookup.Add(item.itemName, item);
            }
        }
    }

    public InventoryItemData GetItemByName(string name)
    {
        if (itemLookup == null)
        {
            BuildLookup();
        }

        if (itemLookup.TryGetValue(name, out InventoryItemData item))
        {
            return item;
        }

        Debug.LogWarning($"Item with name '{name}' not found in ItemDatabase.");
        return null;
    }

    // Singleton-style access
    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                if (_instance == null)
                {
                    Debug.LogError("ItemDatabase not found in Resources folder!");
                }
                else
                {
                    _instance.BuildLookup();
                }
            }
            return _instance;
        }
    }
}
