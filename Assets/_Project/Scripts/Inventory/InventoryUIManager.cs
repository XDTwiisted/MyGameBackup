using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        // Restore last category if saved
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

        List<InventoryEntry> items = InventoryManager.Instance.GetInventory(currentCategory);
        foreach (var entry in items)
        {
            GameObject prefab = GetSlotPrefabForCategory(entry.itemData.category);
            if (prefab == null) continue;

            GameObject slotGO = GameObject.Instantiate(prefab, itemListParent);
            InventoryItemUI itemUI = slotGO.GetComponent<InventoryItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(entry.itemData, entry.quantity);
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
        // Optional: visually highlight active button (could use color or interactable toggle)
        weaponsTabButton.interactable = category != "Weapon";
        toolsTabButton.interactable = category != "Tool";
        foodTabButton.interactable = category != "Food";
        miscTabButton.interactable = category != "Misc";
        healthTabButton.interactable = category != "Health";
    }
}
