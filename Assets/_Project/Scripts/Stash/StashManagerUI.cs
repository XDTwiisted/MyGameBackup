using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StashManagerUI : MonoBehaviour
{
    public static StashManagerUI Instance;

    [Header("UI References")]
    public GameObject stashPanel;
    public Transform stashContentParent;

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

    [Header("Tab Colors")]
    public Color selectedColor = Color.white;
    public Color defaultColor = new Color(1, 1, 1, 0.4f);

    private string currentCategory = "Food";
    private Dictionary<string, Button> tabButtons;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        tabButtons = new Dictionary<string, Button>
        {
            { "Weapon", weaponsTabButton },
            { "Tool", toolsTabButton },
            { "Food", foodTabButton },
            { "Misc", miscTabButton },
            { "Health", healthTabButton }
        };

        weaponsTabButton.onClick.AddListener(() => ShowCategory("Weapon"));
        toolsTabButton.onClick.AddListener(() => ShowCategory("Tool"));
        foodTabButton.onClick.AddListener(() => ShowCategory("Food"));
        miscTabButton.onClick.AddListener(() => ShowCategory("Misc"));
        healthTabButton.onClick.AddListener(() => ShowCategory("Health"));

        ShowCategory(currentCategory);
    }

    public void ShowCategory(string category)
    {
        currentCategory = category;
        RefreshStashUI();
        UpdateTabColors();
    }

    private void UpdateTabColors()
    {
        foreach (var pair in tabButtons)
        {
            var targetColor = (pair.Key == currentCategory) ? selectedColor : defaultColor;
            var image = pair.Value.GetComponent<Image>();
            if (image != null)
            {
                image.color = targetColor;
            }
        }
    }

    public void RefreshStashUI()
    {
        ClearUI();

        var stash = StashManager.Instance;
        if (stash == null) return;

        foreach (var kvp in stash.GetAllStackables())
        {
            InventoryItemData item = kvp.Key;
            int quantity = kvp.Value;
            if (!IsInCategory(item)) continue;

            GameObject slot = CreateSlotForItem(item);
            if (slot == null) continue;

            InventoryItemUI ui = slot.GetComponent<InventoryItemUI>();
            if (ui != null)
            {
                ui.SetItem(item, quantity);
            }
        }

        foreach (var instance in stash.GetAllInstances())
        {
            InventoryItemData item = instance.itemData;
            if (!IsInCategory(item)) continue;

            GameObject slot = CreateSlotForItem(item);
            if (slot == null) continue;

            InventoryItemUI ui = slot.GetComponent<InventoryItemUI>();
            if (ui != null)
            {
                ui.SetItemInstance(instance);
            }
        }
    }

    public void RefreshStashDisplay(string category = "All")
    {
        ClearUI();

        var stash = StashManager.Instance;
        if (stash == null) return;

        foreach (var kvp in stash.GetAllStackables())
        {
            InventoryItemData item = kvp.Key;
            int quantity = kvp.Value;
            if (category != "All" && !item.category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
                continue;

            GameObject slot = CreateSlotForItem(item);
            if (slot == null) continue;

            InventoryItemUI ui = slot.GetComponent<InventoryItemUI>();
            if (ui != null)
            {
                ui.SetItem(item, quantity);
            }
        }

        foreach (var instance in stash.GetAllInstances())
        {
            InventoryItemData item = instance.itemData;
            if (category != "All" && !item.category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
                continue;

            GameObject slot = CreateSlotForItem(item);
            if (slot == null) continue;

            InventoryItemUI ui = slot.GetComponent<InventoryItemUI>();
            if (ui != null)
            {
                ui.SetItemInstance(instance);
            }
        }
    }

    private bool IsInCategory(InventoryItemData item)
    {
        return item.category.Equals(currentCategory, System.StringComparison.OrdinalIgnoreCase);
    }

    private GameObject CreateSlotForItem(InventoryItemData item)
    {
        GameObject prefab = GetPrefabForCategory(item.category);
        if (prefab == null) return null;

        GameObject obj = Instantiate(prefab, stashContentParent);
        obj.transform.localScale = Vector3.one;
        return obj;
    }

    private GameObject GetPrefabForCategory(string category)
    {
        switch (category.ToLower())
        {
            case "food": return foodSlotPrefab;
            case "health": return healthSlotPrefab;
            case "tool": return toolSlotPrefab;
            case "weapon": return weaponSlotPrefab;
            case "misc": return miscSlotPrefab;
            default: return miscSlotPrefab;
        }
    }

    private void ClearUI()
    {
        foreach (Transform child in stashContentParent)
        {
            Destroy(child.gameObject);
        }
    }
}
