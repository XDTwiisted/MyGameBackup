using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject foodSlotPrefab;
    public GameObject healthSlotPrefab;
    public GameObject toolSlotPrefab;
    public GameObject weaponSlotPrefab;
    public GameObject miscSlotPrefab;

    [Header("Optional UI Labels")]
    public GameObject foodHeaderPrefab;
    public GameObject healthHeaderPrefab;
    public GameObject thirstHeaderPrefab;
    public GameObject miscHeaderPrefab;
    public GameObject toolsHeaderPrefab;
    public GameObject weaponsHeaderPrefab;

    [Header("UI Parent for Item Slots")]
    public Transform contentParent;

    // Stackable (non-durable) inventory
    public List<InventoryEntry> inventory = new List<InventoryEntry>();

    // Unique (durable) runtime-only inventory
    public List<ItemInstance> runtimeInventory = new List<ItemInstance>();

    private string currentCategory = "All";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ItemDatabase.Initialize();

        inventory = SaveManager.LoadInventory();
        runtimeInventory = SaveManager.LoadRuntimeInventory();

        foreach (var entry in inventory)
        {
            Debug.Log("Loaded stackable: " + entry.itemData.itemName + " x" + entry.quantity);
        }

        foreach (var instance in runtimeInventory)
        {
            Debug.Log("Loaded durable: " + instance.itemData.itemName + " (Durability: " + instance.currentDurability + ")");
        }

        RefreshUI();
    }

    private void OnApplicationQuit()
    {
        SaveInventoryNow();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveInventoryNow();
        }
    }

    public void AddItem(InventoryItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            Debug.LogWarning("Invalid item or amount in AddItem");
            return;
        }

        if (item.isDurable)
        {
            for (int i = 0; i < amount; i++)
            {
                ItemInstance instance = new ItemInstance(item, 1, item.maxDurability);
                runtimeInventory.Add(instance);
                Debug.Log($"Added durable item: {item.itemName} (Durability: {instance.currentDurability})");
            }
        }
        else
        {
            InventoryEntry existing = inventory.Find(i => i.itemData.itemID == item.itemID);
            if (existing != null)
            {
                existing.quantity += amount;
            }
            else
            {
                inventory.Add(new InventoryEntry(item, amount));
            }
        }

        SaveInventoryNow();
        RefreshUI();
    }

    public void AddItemInstance(ItemInstance newItem)
    {
        if (newItem == null || newItem.itemData == null)
        {
            Debug.LogWarning("Null ItemInstance passed to AddItemInstance.");
            return;
        }

        if (!newItem.itemData.isDurable)
        {
            InventoryEntry existing = inventory.Find(i => i.itemData.itemID == newItem.itemData.itemID);
            if (existing != null)
            {
                existing.quantity += newItem.quantity;
            }
            else
            {
                inventory.Add(new InventoryEntry(newItem.itemData, newItem.quantity));
            }
        }
        else
        {
            runtimeInventory.Add(newItem);
        }

        SaveInventoryNow();
        RefreshUI();
        Debug.Log($"Added {newItem.quantity}x {newItem.itemData.itemName} (Durability: {newItem.currentDurability})");
    }

    public void UseItem(InventoryItemData item)
    {
        if (item == null || item.isDurable)
        {
            Debug.LogWarning("UseItem called with null or durable item.");
            return;
        }

        InventoryEntry entry = inventory.Find(i => i.itemData.itemID == item.itemID);
        if (entry != null)
        {
            entry.quantity--;

            if (entry.quantity <= 0)
                inventory.Remove(entry);

            SaveInventoryNow();
            RefreshUI();
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
        runtimeInventory.Clear();

        PlayerPrefs.DeleteKey("inventory");
        PlayerPrefs.DeleteKey("runtimeInventory");
        PlayerPrefs.Save();

        Debug.Log("Inventory cleared.");

        RefreshUI();
    }

    public void ClearAllItems()
    {
        inventory.Clear();
        runtimeInventory.Clear();
    }

    public (List<InventoryEntry>, List<ItemInstance>) GetAllItems()
    {
        return (new List<InventoryEntry>(inventory), new List<ItemInstance>(runtimeInventory));
    }

    public string CurrentCategory => currentCategory;

    public void SetCategory(string category)
    {
        currentCategory = category;
    }

    public List<InventoryEntry> GetInventory(string category = "All")
    {
        if (category == "All") return inventory;
        return inventory.FindAll(entry => entry.itemData.category == category);
    }

    public List<ItemInstance> GetRuntimeInventory(string category = "All")
    {
        if (category == "All") return runtimeInventory;
        return runtimeInventory.FindAll(entry => entry.itemData.category == category);
    }

    private void RefreshUI()
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.RefreshInventoryDisplay();

        if (StashManagerUI.Instance != null)
            StashManagerUI.Instance.RefreshStashUI();
    }

    private void SaveInventoryNow()
    {
        SaveManager.SaveInventory(inventory);
        SaveManager.SaveRuntimeInventory(runtimeInventory);
    }
}
