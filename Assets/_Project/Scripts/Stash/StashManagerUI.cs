using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StashManagerUI : MonoBehaviour
{
    public static StashManagerUI Instance;

    [Header("UI References")]
    public GameObject stashPanel;
    public Transform stashContentParent; // Should have GridLayoutGroup + ContentSizeFitter

    [Header("Slot Prefabs (existing, unchanged)")]
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

    [Header("Behavior")]
    [Tooltip("Force panel active on Start (useful for 'always show in bunker').")]
    public bool forceActiveOnStart = false;

    [Tooltip("If true, stackable items will repeat an icon per quantity (e.g., qty=3 -> 3 icons).")]
    public bool repeatIconsByQuantity = false;

    private string currentCategory = "Food";
    private Dictionary<string, Button> tabButtons;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (forceActiveOnStart && stashPanel != null)
            stashPanel.SetActive(true);

        // Safety: ensure Content is configured sanely for a grid
        EnsureContentLayoutSafety();

        tabButtons = new Dictionary<string, Button>
        {
            { "Weapon", weaponsTabButton },
            { "Tool",   toolsTabButton },
            { "Food",   foodTabButton },
            { "Misc",   miscTabButton },
            { "Health", healthTabButton }
        };

        if (weaponsTabButton != null) weaponsTabButton.onClick.AddListener(() => ShowCategory("Weapon"));
        if (toolsTabButton != null) toolsTabButton.onClick.AddListener(() => ShowCategory("Tool"));
        if (foodTabButton != null) foodTabButton.onClick.AddListener(() => ShowCategory("Food"));
        if (miscTabButton != null) miscTabButton.onClick.AddListener(() => ShowCategory("Misc"));
        if (healthTabButton != null) healthTabButton.onClick.AddListener(() => ShowCategory("Health"));

        ShowCategory(currentCategory);
    }

    private void OnEnable()
    {
        RefreshStashUI();
        UpdateTabColors();
    }

    // ---------- Public Controls ----------

    public void ShowCategory(string category)
    {
        currentCategory = category;
        RefreshStashUI();
        UpdateTabColors();
    }

    public void ForceRefresh()
    {
        RefreshStashUI();
        UpdateTabColors();
    }

    // ---------- Visual Tabs ----------

    private void UpdateTabColors()
    {
        if (tabButtons == null) return;

        foreach (var pair in tabButtons)
        {
            var btn = pair.Value;
            if (btn == null) continue;

            var image = btn.GetComponent<Image>();
            if (image != null)
                image.color = (pair.Key == currentCategory) ? selectedColor : defaultColor;
        }
    }

    // ---------- Icon-Only Rendering ----------

    public void RefreshStashUI()
    {
        ClearUI();

        var stash = StashManager.Instance;
        if (stash == null || stashContentParent == null) return;

        int added = 0;
        int stackablesConsidered = 0;
        int durablesConsidered = 0;

        // Stackables
        foreach (var kvp in stash.GetAllStackables())
        {
            var item = kvp.Key;
            var qty = kvp.Value;
            if (!IsInCategory(item)) continue;
            stackablesConsidered++;

            if (repeatIconsByQuantity && qty > 1)
            {
                for (int i = 0; i < qty; i++)
                {
                    AddIconEntry(item);
                    added++;
                }
            }
            else
            {
                AddIconEntry(item);
                added++;
            }
        }

        // Durables
        foreach (var instance in stash.GetAllInstances())
        {
            var item = instance.itemData;
            if (!IsInCategory(item)) continue;
            durablesConsidered++;

            AddIconEntry(item);
            added++;
        }

        // Force a full layout pass so multiple icons actually appear
        ForceLayoutPass();

        Debug.Log($"[StashUI] Built icons: {added} (stackables considered: {stackablesConsidered}, durables considered: {durablesConsidered}). Children now: {stashContentParent.childCount}");
    }

    public void RefreshStashDisplay(string category = "All")
    {
        ClearUI();

        var stash = StashManager.Instance;
        if (stash == null || stashContentParent == null) return;

        int added = 0;
        int stackablesConsidered = 0;
        int durablesConsidered = 0;

        // Stackables
        foreach (var kvp in stash.GetAllStackables())
        {
            var item = kvp.Key;
            var qty = kvp.Value;
            if (category != "All" && !CategoryMatches(item.category, category)) continue;
            stackablesConsidered++;

            if (repeatIconsByQuantity && qty > 1)
            {
                for (int i = 0; i < qty; i++)
                {
                    AddIconEntry(item);
                    added++;
                }
            }
            else
            {
                AddIconEntry(item);
                added++;
            }
        }

        // Durables
        foreach (var instance in stash.GetAllInstances())
        {
            var item = instance.itemData;
            if (category != "All" && !CategoryMatches(item.category, category)) continue;
            durablesConsidered++;

            AddIconEntry(item);
            added++;
        }

        ForceLayoutPass();

        Debug.Log($"[StashUI] Built icons (RefreshStashDisplay): {added} (stackables considered: {stackablesConsidered}, durables considered: {durablesConsidered}). Children now: {stashContentParent.childCount}");
    }

    private bool IsInCategory(InventoryItemData item)
    {
        // Allow loose matching (“Food/Thirst” should match “Food”, etc.)
        return CategoryMatches(item.category, currentCategory);
    }

    private bool CategoryMatches(string itemCategory, string wanted)
    {
        if (string.IsNullOrEmpty(wanted) || wanted.Equals("All", System.StringComparison.OrdinalIgnoreCase))
            return true;

        var icat = (itemCategory ?? "").Trim().ToLowerInvariant();
        var wcat = wanted.Trim().ToLowerInvariant();

        return icat == wcat || icat.StartsWith(wcat) || icat.Contains(wcat);
    }

    private void AddIconEntry(InventoryItemData itemData)
    {
        var slot = CreateSlotForItem(itemData);
        if (slot == null) return;

        ConfigureAsIconOnly(slot, itemData.icon);
    }

    // Instantiate your existing per-category slot prefabs (unchanged),
    // then immediately strip them down to icon-only in ConfigureAsIconOnly.
    private GameObject CreateSlotForItem(InventoryItemData item)
    {
        GameObject prefab = GetPrefabForCategory(item.category);
        if (prefab == null || stashContentParent == null) return null;

        GameObject obj = Instantiate(prefab, stashContentParent);
        obj.transform.localScale = Vector3.one;

        // Safety: ensure the child participates in layout
        var le = obj.GetComponent<LayoutElement>();
        if (le == null) le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 64;   // match your Grid cell size
        le.preferredHeight = 64;

        // Ensure RectTransform has sane anchors for grid children
        var rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(64, 64);
        }

        return obj;
    }

    private GameObject GetPrefabForCategory(string category)
    {
        switch ((category ?? "").ToLower())
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
        if (stashContentParent == null) return;
        for (int i = stashContentParent.childCount - 1; i >= 0; i--)
            Destroy(stashContentParent.GetChild(i).gameObject);
    }

    // ---------- Helpers: Icon-Only Transformation ----------

    private bool TryGetIconImage(GameObject slot, out Image iconImage)
    {
        iconImage = null;

        // 1) Prefer InventoryItemUI.iconImage if present
        var ui = slot.GetComponent<InventoryItemUI>();
        if (ui != null && ui.iconImage != null)
        {
            iconImage = ui.iconImage;
            return true;
        }

        // 2) Try a child named "Icon"
        var iconTransform = slot.transform.Find("Icon");
        if (iconTransform != null)
        {
            var img = iconTransform.GetComponent<Image>();
            if (img != null)
            {
                iconImage = img;
                return true;
            }
        }

        // 3) Fallback: first Image found
        iconImage = slot.GetComponentInChildren<Image>(includeInactive: true);
        return iconImage != null;
    }

    private void ConfigureAsIconOnly(GameObject slot, Sprite sprite)
    {
        // Set the icon
        if (TryGetIconImage(slot, out var iconImg))
        {
            iconImg.sprite = sprite;
            iconImg.preserveAspect = true;
            iconImg.enabled = true;
        }

        // Hide all TMP texts and legacy Text
        foreach (var tmp in slot.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true))
            tmp.gameObject.SetActive(false);
        foreach (var txt in slot.GetComponentsInChildren<Text>(includeInactive: true))
            txt.gameObject.SetActive(false);

        // Hide all sliders (durability etc.)
        foreach (var slider in slot.GetComponentsInChildren<Slider>(includeInactive: true))
            slider.gameObject.SetActive(false);

        // Keep the root button clickable but visually neutral (optional)
        var btn = slot.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = true;
            btn.transition = Selectable.Transition.None;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            btn.colors = colors;
        }

        // Hide any non-icon Images (keep the chosen icon)
        foreach (var img in slot.GetComponentsInChildren<Image>(includeInactive: true))
        {
            if (img == iconImg) continue;
            img.enabled = false; // comment out if you want to keep a background frame
        }

        // Disable InventoryItemUI so it doesn’t try to drive texts
        var inventoryUI = slot.GetComponent<InventoryItemUI>();
        if (inventoryUI != null)
            inventoryUI.enabled = false;

        // Hard-size to grid cell via LayoutElement in CreateSlotForItem()
    }

    // ---------- Layout Safety ----------

    private void EnsureContentLayoutSafety()
    {
        if (stashContentParent == null) return;

        var contentRT = stashContentParent as RectTransform;
        if (contentRT != null)
        {
            // Stretch horizontally, top-anchored so it grows downward
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
        }

        // Ensure Grid + CSF exist and are sane
        var grid = stashContentParent.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = stashContentParent.gameObject.AddComponent<GridLayoutGroup>();
        if (grid != null)
        {
            if (grid.cellSize == Vector2.zero) grid.cellSize = new Vector2(64, 64);
            if (grid.spacing == Vector2.zero) grid.spacing = new Vector2(6, 6);
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            if (grid.constraintCount < 1) grid.constraintCount = 4; // default to 4 columns
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        }

        var csf = stashContentParent.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = stashContentParent.gameObject.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void ForceLayoutPass()
    {
        // Force Unity to rebuild the grid layout immediately so more than one icon actually shows.
        var rt = stashContentParent as RectTransform;
        if (rt != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            Canvas.ForceUpdateCanvases();
        }
    }
}
