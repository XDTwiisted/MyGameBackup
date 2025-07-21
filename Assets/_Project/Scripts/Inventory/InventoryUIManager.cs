using UnityEngine;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform itemListParent;

    [Header("Slot Prefabs")]
    public GameObject weaponSlotPrefab;
    public GameObject toolSlotPrefab;
    public GameObject healthSlotPrefab;
    public GameObject miscSlotPrefab;
    public GameObject foodThirstSlotPrefab; // Shared prefab for Food and Thirst items

    private bool isVisible = false;
    private string currentCategoryFilter = "Food";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleInventory()
    {
        isVisible = !isVisible;

        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryUIManager: inventoryPanel is not assigned!");
            return;
        }

        inventoryPanel.SetActive(isVisible);

        if (!isVisible) return;

        if (InventoryManager.Instance != null)
        {
            currentCategoryFilter = InventoryManager.Instance.CurrentCategory;
            InventoryManager.Instance.SetCategory(currentCategoryFilter);
        }
        else
        {
            Debug.LogError("InventoryUIManager: InventoryManager.Instance is null!");
        }

        if (InventoryCategoryGroup.Instance != null)
        {
            InventoryCategoryGroup.Instance.SetActiveCategory(currentCategoryFilter);
        }

        RefreshInventoryDisplay();
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
        if (itemListParent == null)
        {
            Debug.LogError("InventoryUIManager: itemListParent is not assigned!");
            return;
        }

        foreach (Transform child in itemListParent)
        {
            Destroy(child.gameObject);
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryUIManager: InventoryManager.Instance is null during refresh.");
            return;
        }

        var inventory = InventoryManager.Instance.GetInventory();

        foreach (var pair in inventory)
        {
            InventoryItemData item = pair.Key;
            int quantity = pair.Value;

            if (item == null)
            {
                Debug.LogWarning("InventoryUIManager: Skipping null item in inventory.");
                continue;
            }

            if (item.category != currentCategoryFilter)
                continue;

            GameObject prefabToUse = GetSlotPrefabByCategory(item.category);

            if (prefabToUse == null)
            {
                Debug.LogError($"InventoryUIManager: No prefab found for category '{item.category}'");
                continue;
            }

            GameObject newItem = Instantiate(prefabToUse, itemListParent);
            InventoryItemUI itemUI = newItem.GetComponent<InventoryItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(item, quantity);
            }
            else
            {
                Debug.LogError("InventoryUIManager: Instantiated prefab is missing InventoryItemUI component.");
            }
        }
    }

    private GameObject GetSlotPrefabByCategory(string category)
    {
        switch (category)
        {
            case "Weapon": return weaponSlotPrefab;
            case "Tool": return toolSlotPrefab;
            case "Health": return healthSlotPrefab;
            case "Misc": return miscSlotPrefab;
            case "Food":
            case "Thirst":
                return foodThirstSlotPrefab;
            default:
                Debug.LogWarning($"InventoryUIManager: Unknown category '{category}'");
                return null;
        }
    }
}
