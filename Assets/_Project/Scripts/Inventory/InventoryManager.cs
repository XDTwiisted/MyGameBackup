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

    [Header("Category Headers (Optional UI Labels)")]
    public GameObject foodHeaderPrefab;
    public GameObject healthHeaderPrefab;
    public GameObject thirstHeaderPrefab;
    public GameObject miscHeaderPrefab;
    public GameObject toolsHeaderPrefab;
    public GameObject weaponsHeaderPrefab;

    [Header("UI Parent")]
    public Transform contentParent;

    [Header("Item List (for Database)")]
    public List<InventoryItemData> allItems; // Assign all ScriptableObjects here

    public List<InventoryEntry> inventory = new List<InventoryEntry>();

    private string currentCategory = "All";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ItemDatabase.Initialize(allItems);
        inventory = SaveManager.LoadInventory();

        foreach (var entry in inventory)
        {
            Debug.Log($"Loaded: {entry.itemData.itemName} x{entry.quantity}");
        }
    }

    private void OnApplicationQuit()
    {
        SaveManager.SaveInventory(inventory);
    }

    public void AddItem(InventoryItemData item, int amount)
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

    public void UseItem(InventoryItemData item)
    {
        InventoryEntry entry = inventory.Find(i => i.itemData.itemID == item.itemID);
        if (entry != null)
        {
            entry.quantity--;

            if (entry.quantity <= 0)
            {
                inventory.Remove(entry);
            }

            InventoryUIManager.Instance?.RefreshInventoryDisplay();
        }
    }

    public void ClearInventory()
    {
        inventory.Clear();
        SaveManager.SaveInventory(inventory);
        InventoryUIManager.Instance?.RefreshInventoryDisplay();
        Debug.Log("Inventory cleared.");
    }
}
