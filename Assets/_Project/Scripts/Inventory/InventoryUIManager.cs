using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform itemListParent;

    [Header("Slot Prefabs")]
    public GameObject foodSlotPrefab;
    public GameObject healthSlotPrefab;
    public GameObject toolSlotPrefab;
    public GameObject weaponSlotPrefab;
    public GameObject miscSlotPrefab;

    [Header("Tab Buttons")]
    public Button weaponsTabButton;
    public Button toolsTabButton;
    public Button foodTabButton;
    public Button miscTabButton;
    public Button healthTabButton;

    private string currentCategory = "All";
    private const string LastCategoryKey = "LastInventoryCategory";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        weaponsTabButton.onClick.AddListener(() => OnTabClicked("Weapon"));
        toolsTabButton.onClick.AddListener(() => OnTabClicked("Tool"));
        foodTabButton.onClick.AddListener(() => OnTabClicked("Food"));
        miscTabButton.onClick.AddListener(() => OnTabClicked("Misc"));
        healthTabButton.onClick.AddListener(() => OnTabClicked("Health"));

        inventoryPanel.SetActive(false);

        if (PlayerPrefs.HasKey(LastCategoryKey))
        {
            currentCategory = PlayerPrefs.GetString(LastCategoryKey);
        }
    }

    public void ToggleInventory()
    {
        bool isVisible = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isVisible);

        if (!isVisible)
        {
            ShowCategory(currentCategory);
        }
    }

    public void OnTabClicked(string category)
    {
        PlayerPrefs.SetString(LastCategoryKey, category);
        PlayerPrefs.Save();
        ShowCategory(category);
    }

    public void ShowCategory(string category)
    {
        currentCategory = category;
        InventoryManager.Instance.SetCategory(category);
        RefreshInventoryDisplay();
        HighlightActiveTab(category);
    }

    public void RefreshInventoryDisplay()
    {
        foreach (Transform child in itemListParent)
        {
            Destroy(child.gameObject);
        }

        string category = currentCategory;

        // Handle durable items (e.g., weapons/tools)
        List<ItemInstance> durableItems = InventoryManager.Instance.GetRuntimeInventory(category);
        foreach (var item in durableItems)
        {
            GameObject prefab = GetSlotPrefabForCategory(item.itemData.category);
            if (prefab == null) continue;

            GameObject slotGO = Instantiate(prefab, itemListParent);
            InventoryItemUI itemUI = slotGO.GetComponent<InventoryItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(item.itemData, item.quantity, item.currentDurability);
            }
        }

        // Handle non-durable stackables (e.g., food/health)
        List<InventoryEntry> stackableItems = InventoryManager.Instance.GetInventory(category);
        foreach (var entry in stackableItems)
        {
            GameObject prefab = GetSlotPrefabForCategory(entry.itemData.category);
            if (prefab == null) continue;

            GameObject slotGO = Instantiate(prefab, itemListParent);
            InventoryItemUI itemUI = slotGO.GetComponent<InventoryItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(entry.itemData, entry.quantity, -1);
            }
        }
    }

    private GameObject GetSlotPrefabForCategory(string category)
    {
        switch (category)
        {
            case "Food": return foodSlotPrefab;
            case "Health": return healthSlotPrefab;
            case "Tool": return toolSlotPrefab;
            case "Weapon": return weaponSlotPrefab;
            case "Misc": return miscSlotPrefab;
            default: return null;
        }
    }

    private void HighlightActiveTab(string category)
    {
        weaponsTabButton.interactable = category != "Weapon";
        toolsTabButton.interactable = category != "Tool";
        foodTabButton.interactable = category != "Food";
        miscTabButton.interactable = category != "Misc";
        healthTabButton.interactable = category != "Health";
    }
}
