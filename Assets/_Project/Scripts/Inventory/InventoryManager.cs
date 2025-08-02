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

    public List<InventoryEntry> inventory = new List<InventoryEntry>();
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

        RefreshUI();
    }

    private void OnApplicationQuit()
    {
        SaveInventoryNow();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveInventoryNow();
    }

    public void AddItem(InventoryItemData item, int amount)
    {
        if (item == null || amount <= 0) return;

        if (item.isDurable)
        {
            for (int i = 0; i < amount; i++)
                runtimeInventory.Add(new ItemInstance(item, 1, item.maxDurability));
        }
        else
        {
            InventoryEntry existing = inventory.Find(i => i.itemData.itemID == item.itemID);
            if (existing != null)
                existing.quantity += amount;
            else
                inventory.Add(new InventoryEntry(item, amount));
        }

        SaveInventoryNow();
        RefreshUI();
    }

    public void AddItemInstance(ItemInstance newItem)
    {
        if (newItem == null || newItem.itemData == null) return;

        if (!newItem.itemData.isDurable)
        {
            InventoryEntry existing = inventory.Find(i => i.itemData.itemID == newItem.itemData.itemID);
            if (existing != null)
                existing.quantity += newItem.quantity;
            else
                inventory.Add(new InventoryEntry(newItem.itemData, newItem.quantity));
        }
        else
        {
            runtimeInventory.Add(newItem);
        }

        SaveInventoryNow();
        RefreshUI();
    }

    public void UseItem(InventoryItemData item)
    {
        if (item == null || item.isDurable) return;

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
        return category == "All" ? inventory : inventory.FindAll(i => i.itemData.category == category);
    }

    public List<ItemInstance> GetRuntimeInventory(string category = "All")
    {
        return category == "All" ? runtimeInventory : runtimeInventory.FindAll(i => i.itemData.category == category);
    }

    private void RefreshUI()
    {
        InventoryUIManager.Instance?.RefreshInventoryDisplay();
        StashManagerUI.Instance?.RefreshStashUI();
    }

    private void SaveInventoryNow()
    {
        SaveManager.SaveInventory(inventory);
        SaveManager.SaveRuntimeInventory(runtimeInventory);
    }

    //  FIXED: Return bool for success/failure
    public bool RemoveStackableItem(InventoryItemData item, int quantityToRemove)
    {
        InventoryEntry entry = inventory.Find(i => i.itemData.itemID == item.itemID);
        if (entry != null)
        {
            entry.quantity -= quantityToRemove;
            if (entry.quantity <= 0)
                inventory.Remove(entry);

            SaveInventoryNow();
            RefreshUI();
            return true;
        }
        return false;
    }

    public bool RemoveDurableItem(ItemInstance instance)
    {
        if (runtimeInventory.Contains(instance))
        {
            runtimeInventory.Remove(instance);
            SaveInventoryNow();
            RefreshUI();
            return true;
        }
        return false;
    }
}
