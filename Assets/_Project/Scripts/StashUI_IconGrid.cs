using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StashUI_IconGrid : MonoBehaviour
{
    [Header("References")]
    public RectTransform content;        // Scroll View > Viewport > Content (has GridLayoutGroup)
    public RectTransform viewport;       // Scroll View > Viewport
    public ScrollRect scrollRect;        // The ScrollRect on your Scroll View
    public GameObject iconPrefab;        // Prefab with children: Background(Image), Icon(Image)

    [Header("Tabs (optional)")]
    public Button tabAll, tabWeapons, tabTools, tabFood, tabMisc, tabHealth;

    [Header("Display")]
    public float iconPadding = 10f;      // padding inside each cell for the Icon image
    public bool cloneByQuantity = false;
    public bool centerVerticallyWhenShort = true;
    public bool debugLogging = false;

    private string currentCategory = "Food";
    private GridLayoutGroup grid;        // read-only: we do NOT change its settings

    private static readonly string[] FoodSynonyms = { "food", "thirst", "drink", "water", "consumable" };
    private static readonly string[] HealthSynonyms = { "health", "med", "medicine", "bandage" };

    private void Awake()
    {
        if (content != null) grid = content.GetComponent<GridLayoutGroup>();

        // lock horizontal scrolling
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
        }

        WireTabs();
        EnsureContentAnchors();
    }

    private void OnEnable()
    {
        EnsureContentAnchors();
        ShowCategory(currentCategory);
    }

    private void OnRectTransformDimensionsChange()
    {
        EnsureContentAnchors();
        LockContentLeft();
    }

    private void WireTabs()
    {
        if (tabAll) tabAll.onClick.AddListener(() => ShowCategory("All"));
        if (tabWeapons) tabWeapons.onClick.AddListener(() => ShowCategory("Weapon"));
        if (tabTools) tabTools.onClick.AddListener(() => ShowCategory("Tool"));
        if (tabFood) tabFood.onClick.AddListener(() => ShowCategory("Food"));
        if (tabMisc) tabMisc.onClick.AddListener(() => ShowCategory("Misc"));
        if (tabHealth) tabHealth.onClick.AddListener(() => ShowCategory("Health"));
    }

    public void ShowCategory(string category)
    {
        currentCategory = category;
        Refresh();
    }

    public void Refresh()
    {
        if (content == null || iconPrefab == null) return;
        var stash = StashManager.Instance;
        if (stash == null) return;

        // stop any residual motion
        if (scrollRect != null) { scrollRect.StopMovement(); scrollRect.velocity = Vector2.zero; }

        // clear
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        int added = 0;

        // stackables
        foreach (var kvp in stash.GetAllStackables())
        {
            var item = kvp.Key;
            var qty = Mathf.Max(0, kvp.Value);
            if (!MatchesCategory(item.category, currentCategory)) continue;

            int clones = cloneByQuantity ? Mathf.Max(qty, 1) : 1;
            for (int i = 0; i < clones; i++) { AddIcon(item); added++; }
        }

        // durables
        foreach (var inst in stash.GetAllInstances())
        {
            var item = inst.itemData;
            if (!MatchesCategory(item.category, currentCategory)) continue;
            AddIcon(item);
            added++;
        }

        ForceLayoutNow();
        ApplyVerticalCenterIfNeeded();
        LockContentLeft();

        if (debugLogging)
            Debug.Log("[StashUI_IconGrid] '" + currentCategory + "' -> added " + added + " icons. Children=" + content.childCount);
    }

    private void AddIcon(InventoryItemData itemData)
    {
        var go = Instantiate(iconPrefab, content);
        go.name = "StashIcon_" + (itemData != null ? itemData.itemName : "NULL");

        // use current grid cell size from Inspector
        Vector2 cell = GetGridCellSize();

        // slot size
        var slotRT = go.GetComponent<RectTransform>();
        if (slotRT != null) slotRT.sizeDelta = cell;

        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        le.preferredWidth = cell.x;
        le.preferredHeight = cell.y;

        // background fills cell
        var bg = FindChildImage(go.transform, "Background");
        if (bg != null)
        {
            var brt = bg.rectTransform;
            brt.anchorMin = new Vector2(0, 0);
            brt.anchorMax = new Vector2(1, 1);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = Vector2.zero;
            brt.anchoredPosition = Vector2.zero;
            bg.preserveAspect = false;
        }

        // icon centered with padding
        var img = FindChildImage(go.transform, "Icon");
        if (img == null) img = FindChildImage(go.transform, "StashIconItem");
        if (img != null && itemData != null)
        {
            img.sprite = itemData.icon;
            img.preserveAspect = true;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            float w = Mathf.Max(0f, cell.x - iconPadding * 2f);
            float h = Mathf.Max(0f, cell.y - iconPadding * 2f);
            rt.sizeDelta = new Vector2(w, h);
        }
    }

    private Image FindChildImage(Transform root, string childName)
    {
        var t = root.Find(childName);
        return t ? t.GetComponent<Image>() : null;
    }

    private bool MatchesCategory(string itemCategory, string wanted)
    {
        if (string.IsNullOrEmpty(wanted) || wanted.Equals("All", System.StringComparison.OrdinalIgnoreCase))
            return true;

        var icat = (itemCategory ?? "").Trim().ToLowerInvariant();
        var wcat = wanted.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(icat)) return false;

        if (icat == wcat || icat.StartsWith(wcat) || icat.Contains(wcat)) return true;

        if (wcat == "food") { foreach (var s in FoodSynonyms) if (icat.Contains(s)) return true; }
        if (wcat == "health") { foreach (var s in HealthSynonyms) if (icat.Contains(s)) return true; }
        return false;
    }

    private Vector2 GetGridCellSize()
    {
        if (grid != null && grid.cellSize.x > 0f && grid.cellSize.y > 0f) return grid.cellSize;
        // fallback
        return new Vector2(100f, 100f);
    }

    private void EnsureContentAnchors()
    {
        if (content == null) return;
        // stretch horizontally, top anchored; keep left edge fixed
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0f, 1f);
        content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);

        // lock width to viewport to avoid horizontal scrolling
        if (viewport != null)
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewport.rect.width);
    }

    private void LockContentLeft()
    {
        if (content == null) return;
        var pos = content.anchoredPosition;
        if (pos.x != 0f) content.anchoredPosition = new Vector2(0f, pos.y);
    }

    private void ForceLayoutNow()
    {
        if (content == null) return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();
    }

    private void ApplyVerticalCenterIfNeeded()
    {
        if (!centerVerticallyWhenShort || content == null || viewport == null) return;

        float contentHeight = LayoutUtility.GetPreferredHeight(content);
        float viewportHeight = viewport.rect.height;

        if (contentHeight < viewportHeight)
        {
            float gap = viewportHeight - contentHeight;
            content.anchoredPosition = new Vector2(0f, -gap * 0.5f);
        }
        else
        {
            content.anchoredPosition = new Vector2(0f, 0f);
        }
    }
}
