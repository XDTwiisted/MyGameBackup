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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        weaponsTabButton.onClick.AddListener(() => ShowCategory("Weapon"));
        toolsTabButton.onClick.AddListener(() => ShowCategory("Tool"));
        foodTabButton.onClick.AddListener(() => ShowCategory("Food"));
        miscTabButton.onClick.AddListener(() => ShowCategory("Misc"));
        healthTabButton.onClick.AddListener(() => ShowCategory("Health"));

        inventoryPanel.SetActive(false);
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

    public void ShowCategory(string category)
    {
        currentCategory = category;
        InventoryManager.Instance.SetCategory(category);
        RefreshInventoryDisplay();
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

            GameObject slotGO = Instantiate(prefab, itemListParent);
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
}
