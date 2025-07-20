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

    private string currentCategory = "Food";
    private string saveKey = "InventorySaveData";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadInventory();
        RefreshInventoryUI();
    }

    public void SetCategory(string category)
    {
        currentCategory = category;
        RefreshInventoryUI();
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
}
