using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItemSave
{
    public string itemName;
    public int quantity;
}

[System.Serializable]
public class InventorySaveData
{
    public List<InventoryItemSave> items = new List<InventoryItemSave>();
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public GameObject itemSlotPrefab;
    public Transform contentParent;

    public List<InventoryEntry> inventory = new List<InventoryEntry>();

    private string currentCategory = "Food";  // Default starting category
    private readonly string saveKey = "InventorySaveData";
    private readonly string categoryKey = "InventoryCurrentCategory"; // Key for saving category

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved category as early as possible
            LoadCategory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadInventory();
        // Set category after inventory loaded to sync UI and save properly
        SetCategory(currentCategory);
    }

    public string CurrentCategory => currentCategory;

    public void SetCategory(string category)
    {
        currentCategory = category;
        PlayerPrefs.SetString(categoryKey, currentCategory);
        PlayerPrefs.Save();

        RefreshInventoryUI();

        // Highlight the correct category tab
        if (InventoryCategoryGroup.Instance != null)
        {
            InventoryCategoryGroup.Instance.SetActiveCategory(currentCategory);
        }
    }

    private void LoadCategory()
    {
        if (PlayerPrefs.HasKey(categoryKey))
        {
            currentCategory = PlayerPrefs.GetString(categoryKey);
        }
        else
        {
            currentCategory = "Food";  // Default category if nothing saved
        }
    }

    public void RefreshInventoryUI()
    {
        if (contentParent == null)
        {
            Debug.LogError("InventoryManager: contentParent is NOT assigned in the Inspector!");
            return;
        }

        if (itemSlotPrefab == null)
        {
            Debug.LogError("InventoryManager: itemSlotPrefab is NOT assigned in the Inspector!");
            return;
        }

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var entry in inventory)
        {
            if (entry == null || entry.itemData == null)
            {
                Debug.LogWarning("InventoryManager: Skipping null entry or itemData.");
                continue;
            }

            if (entry.itemData.category == currentCategory && entry.quantity > 0)
            {
                GameObject newItemSlot = Instantiate(itemSlotPrefab, contentParent);
                InventoryItemUI itemUI = newItemSlot.GetComponent<InventoryItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(entry.itemData, entry.quantity);
                }
                else
                {
                    Debug.LogError("InventoryManager: itemSlotPrefab is missing InventoryItemUI script!");
                }
            }
        }
    }

    public void AddItem(InventoryItemData itemData, int quantity)
    {
        if (itemData == null)
        {
            Debug.LogError("InventoryManager: Tried to add null itemData.");
            return;
        }

        Debug.Log($"InventoryManager: Adding item '{itemData.itemName}' x{quantity}");

        var existingEntry = inventory.Find(entry => entry.itemData == itemData);
        if (existingEntry != null)
        {
            existingEntry.quantity += quantity;
        }
        else
        {
            inventory.Add(new InventoryEntry(itemData, quantity));
        }

        RefreshInventoryUI();
        SaveInventory();
    }

    public void RemoveItem(InventoryItemData itemData, int quantity)
    {
        var entry = inventory.Find(e => e.itemData == itemData);
        if (entry != null)
        {
            entry.quantity -= quantity;
            if (entry.quantity <= 0)
            {
                inventory.Remove(entry);
            }
        }

        RefreshInventoryUI();
        SaveInventory();
    }

    public void UseItem(InventoryItemData itemData)
    {
        RemoveItem(itemData, 1);
    }

    public void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();

        foreach (var entry in inventory)
        {
            if (entry != null && entry.itemData != null)
            {
                saveData.items.Add(new InventoryItemSave
                {
                    itemName = entry.itemData.itemName,
                    quantity = entry.quantity
                });
            }
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            inventory.Clear();

            foreach (var item in saveData.items)
            {
                InventoryItemData itemData = ItemDatabase.Instance.GetItemByName(item.itemName);
                if (itemData != null)
                {
                    inventory.Add(new InventoryEntry(itemData, item.quantity));
                }
                else
                {
                    Debug.LogWarning($"InventoryManager: Item not found in database: {item.itemName}");
                }
            }
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
        RefreshInventoryUI();
        SaveInventory();
        Debug.Log("Inventory has been cleared.");
    }

    public Dictionary<InventoryItemData, int> GetInventory()
    {
        Dictionary<InventoryItemData, int> inventoryDict = new Dictionary<InventoryItemData, int>();

        foreach (var entry in inventory)
        {
            if (entry != null && entry.itemData != null && entry.quantity > 0)
                inventoryDict[entry.itemData] = entry.quantity;
        }

        return inventoryDict;
    }
}
