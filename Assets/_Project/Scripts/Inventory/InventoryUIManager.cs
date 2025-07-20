using UnityEngine;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform itemListParent;
    public GameObject itemUIPrefab;

    private bool isVisible = false;
    private string currentCategoryFilter;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Initial fallback if InventoryManager not ready yet
        currentCategoryFilter = "Food";
    }

    public void ToggleInventory()
    {
        isVisible = !isVisible;
        inventoryPanel.SetActive(isVisible);

        if (isVisible)
        {
            // Always fetch current saved category from InventoryManager
            if (InventoryManager.Instance != null)
            {
                currentCategoryFilter = InventoryManager.Instance.CurrentCategory;
                InventoryManager.Instance.SetCategory(currentCategoryFilter);
            }

            if (InventoryCategoryGroup.Instance != null)
            {
                InventoryCategoryGroup.Instance.SetActiveCategory(currentCategoryFilter);
            }

            RefreshInventoryDisplay();
        }
    }

    public void ShowCategory(string category)
    {
        currentCategoryFilter = category;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetCategory(category);
        }

        if (InventoryCategoryGroup.Instance != null)
        {
            InventoryCategoryGroup.Instance.SetActiveCategory(category);
        }

        RefreshInventoryDisplay();
    }

    public void RefreshInventoryDisplay()
    {
        foreach (Transform child in itemListParent)
        {
            Destroy(child.gameObject);
        }

        var inventory = InventoryManager.Instance.GetInventory();

        foreach (var pair in inventory)
        {
            InventoryItemData item = pair.Key;
            int quantity = pair.Value;

            if (item.category != currentCategoryFilter)
                continue;

            GameObject newItem = Instantiate(itemUIPrefab, itemListParent);
            newItem.GetComponent<InventoryItemUI>().Setup(item, quantity);
        }
    }
}
