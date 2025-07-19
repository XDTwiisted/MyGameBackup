using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public GameObject itemSlotPrefab;  // Assign your ItemSlot prefab in Inspector
    public Transform contentParent;    // Assign the ScrollView's Content object in Inspector

    public List<InventoryItemData> allItems = new List<InventoryItemData>();

    private string currentCategory = "Food"; // Default category

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        RefreshInventoryUI();
    }

    // Call this to change tabs (e.g., Food, Tools, Weapons, Misc)
    public void SetCategory(string category)
    {
        currentCategory = category;
        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        // Clear previous item slots
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Populate item slots that match current category and have quantity > 0
        foreach (var item in allItems)
        {
            if (item.category == currentCategory && item.quantity > 0)
            {
                GameObject newItemSlot = Instantiate(itemSlotPrefab, contentParent);
                InventoryItemUI itemUI = newItemSlot.GetComponent<InventoryItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(item);
                }
            }
        }
    }

    // Optional: Call to force-remove item after use (if quantity hits 0)
    public void RemoveItem(InventoryItemData item)
    {
        item.quantity = 0;
        RefreshInventoryUI();
    }
}
