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

    [Header("Prefabs")]
    public GameObject itemSlotPrefab;
    public Transform contentParent;

    [Header("Category Headers")]
    public GameObject foodHeaderPrefab;
    public GameObject healthHeaderPrefab;
    public GameObject thirstHeaderPrefab;
    public GameObject miscHeaderPrefab;
    public GameObject toolsHeaderPrefab;
    public GameObject weaponsHeaderPrefab;

    public List<InventoryEntry> inventory = new List<InventoryEntry>();

    private string currentCategory = "Food";  // Default starting category
    private readonly string saveKey = "InventorySaveData";
    private readonly string categoryKey = "InventoryCurrentCategory";

    private Dictionary<string, GameObject> headerPrefabs;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCategory();
            InitializeHeaderMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadInventory();
        SetCategory(currentCategory);
    }

    private void InitializeHeaderMap()
    {
        headerPrefabs = new Dictionary<string, GameObject>
        {
            { "Food", foodHeaderPrefab },
            { "Thirst", thirstHeaderPrefab },
            { "Health", healthHeaderPrefab },
            { "Misc", miscHeaderPrefab },
            { "Tools", toolsHeaderPrefab },
            { "Weapons", weaponsHeaderPrefab }
        };
    }

    public string CurrentCategory => currentCategory;

    public void SetCategory(string category)
    {
        currentCategory = category;
        PlayerPrefs.SetString(categoryKey, currentCategory);
        PlayerPrefs.Save();

        RefreshInventoryUI();

        if (InventoryCategoryGroup.Instance != null)
        {
            InventoryCategoryGroup.Instance.SetActiveCategory(currentCategory);
        }
    }

    private void LoadCategory()
    {
        currentCategory = PlayerPrefs.HasKey(categoryKey)
            ? PlayerPrefs.GetString(categoryKey)
            : "Food";
    }

    public void RefreshInventoryUI()
    {
        if (contentParent == null || itemSlotPrefab == null)
        {
            Debug.LogError("InventoryManager: Missing contentParent or itemSlotPrefab.");
            return;
        }

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"Refreshing Inventory UI (Category: {currentCategory})");

        // Instantiate header (if any for this category)
        if (headerPrefabs != null && headerPrefabs.TryGetValue(currentCategory, out GameObject headerPrefab) && headerPrefab != null)
        {
            Instantiate(headerPrefab, contentParent);
        }

        foreach (var entry in inventory)
        {
            if (entry?.itemData == null) continue;

            if (entry.itemData.category == currentCategory && entry.quantity > 0)
            {
                GameObject newItemSlot = Instantiate(itemSlotPrefab, contentParent);
                InventoryItemUI itemUI = newItemSlot.GetComponent<InventoryItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(entry.itemData, entry.quantity);
                }
            }
        }
    }

    public void AddItem(InventoryItemData itemData, int quantity)
    {
        if (itemData == null) return;

        bool found = false;

        foreach (var entry in inventory)
        {
            if (entry != null && entry.itemData == itemData)
            {
                entry.quantity += quantity;
                found = true;
                break;
            }
        }

        if (!found)
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
            if (entry?.itemData != null)
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
            }
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
        RefreshInventoryUI();
        SaveInventory();
    }

    public Dictionary<InventoryItemData, int> GetInventory()
    {
        Dictionary<InventoryItemData, int> dict = new Dictionary<InventoryItemData, int>();

        foreach (var entry in inventory)
        {
            if (entry?.itemData != null && entry.quantity > 0)
            {
                dict[entry.itemData] = entry.quantity;
            }
        }

        return dict;
    }
}
