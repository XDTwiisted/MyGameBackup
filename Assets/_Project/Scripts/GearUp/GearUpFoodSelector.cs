using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class GearUpFoodSelector : MonoBehaviour
{
    [Header("UI References")]
    public Button addFoodButton;
    public Button backButton;
    public GameObject itemSelectionPanel;
    public TextMeshProUGUI selectionTitle;
    public Transform itemScrollViewContent;
    public GameObject foodSlotPrefab;
    public Transform gearUpFoodContent;

    // Tracks how many of each food have been added to the Gear Up panel
    private readonly Dictionary<InventoryItemData, int> gearUpSelectedCounts = new Dictionary<InventoryItemData, int>();

    void Start()
    {
        if (addFoodButton != null) addFoodButton.onClick.AddListener(OpenFoodSelection);
        if (backButton != null) backButton.onClick.AddListener(CloseFoodSelection);
        if (itemSelectionPanel != null) itemSelectionPanel.SetActive(false);
    }

    private void OpenFoodSelection()
    {
        itemSelectionPanel?.SetActive(true);
        if (selectionTitle != null) selectionTitle.text = "Select Food";

        // Clear old entries
        if (itemScrollViewContent != null)
        {
            for (int i = itemScrollViewContent.childCount - 1; i >= 0; i--)
                Destroy(itemScrollViewContent.GetChild(i).gameObject);
        }

        var stash = StashManager.Instance;
        var stashItems = (stash != null) ? stash.stashItems : null;
        if (stashItems == null) return;

        foreach (var kv in stashItems)
        {
            var data = kv.Key;
            var qty = kv.Value;

            if (data == null || qty <= 0) continue;
            if (!string.Equals(data.category, "Food", StringComparison.OrdinalIgnoreCase)) continue;

            var slotGO = Instantiate(foodSlotPrefab, itemScrollViewContent);
            slotGO.name = "FoodSlot_" + SafeName(data.itemID) + "_" + Guid.NewGuid().ToString("N");

            // Configure list slot visuals (icon, name, quantity, rarity)
            SetupFoodSlotUI(slotGO.transform, data, qty);

            // Selection list shows DropButton normally; RemoveButton not used here
            ShowButton(slotGO.transform, "DropButton", true);
            ShowButton(slotGO.transform, "RemoveButton", false);

            var button = slotGO.GetComponent<Button>();
            if (button != null)
            {
                var captured = data;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => AddFoodToGearUp(captured));
            }
        }

        var rt = (itemScrollViewContent != null) ? itemScrollViewContent.GetComponent<RectTransform>() : null;
        if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    // Adds one unit of the selected food into Gear Up
    private void AddFoodToGearUp(InventoryItemData itemData)
    {
        if (itemData == null) return;

        // 1) Register with selection manager so Confirm moves it into Inventory
        var gsm = FindFirstObjectByType<GearUpSelectionManager>();
        if (gsm != null) gsm.AddStackable(itemData, 1);

        // 2) Update Gear Up counts and UI (aggregate by item)
        int newCount = 1;
        if (gearUpSelectedCounts.TryGetValue(itemData, out var existing))
            newCount = existing + 1;
        gearUpSelectedCounts[itemData] = newCount;

        var slot = FindGearUpSlot(itemData);
        if (slot == null)
        {
            slot = Instantiate(foodSlotPrefab, gearUpFoodContent);
            slot.name = "GearUpFood_" + SafeName(itemData.itemID) + "_" + Guid.NewGuid().ToString("N");

            SetupFoodSlotUI(slot.transform, itemData, newCount);

            // In Gear Up: hide Drop, show Remove, wire Remove to subtract one
            ShowButton(slot.transform, "DropButton", false);
            WireRemoveButton(slot.transform, itemData);
        }
        else
        {
            UpdateFoodSlotQuantity(slot.transform, newCount);
        }

        // 3) Decrement from stash immediately so counts stay correct
        var stash = StashManager.Instance;
        if (stash != null)
        {
            if (!stash.stashItems.ContainsKey(itemData)) stash.stashItems[itemData] = 0;
            stash.stashItems[itemData] = Mathf.Max(0, stash.stashItems[itemData] - 1);
            if (stash.stashItems[itemData] <= 0) stash.stashItems.Remove(itemData);
            StashManagerUI.Instance?.RefreshStashUI();
        }

        // 4) Refresh the selection list so remaining counts update
        if (itemSelectionPanel != null && itemSelectionPanel.activeSelf)
            OpenFoodSelection();
    }

    // Removes one unit of this food from Gear Up and restores it to stash
    private void RemoveFoodFromGearUp(InventoryItemData itemData, int amount = 1)
    {
        if (itemData == null) return;

        // 1) Update selection manager
        var gsm = FindFirstObjectByType<GearUpSelectionManager>();
        if (gsm != null) gsm.RemoveStackable(itemData, amount);

        // 2) Update local counts and UI
        int current;
        if (!gearUpSelectedCounts.TryGetValue(itemData, out current)) return;

        current -= amount;
        if (current > 0)
        {
            gearUpSelectedCounts[itemData] = current;
            var slot = FindGearUpSlot(itemData);
            if (slot != null) UpdateFoodSlotQuantity(slot.transform, current);
        }
        else
        {
            gearUpSelectedCounts.Remove(itemData);
            var slot = FindGearUpSlot(itemData);
            if (slot != null) Destroy(slot);
        }

        // 3) Return to stash
        var stash = StashManager.Instance;
        if (stash != null)
        {
            if (!stash.stashItems.ContainsKey(itemData)) stash.stashItems[itemData] = 0;
            stash.stashItems[itemData] += amount;
            StashManagerUI.Instance?.RefreshStashUI();
        }

        // 4) If selection panel is open, refresh to reflect new counts
        if (itemSelectionPanel != null && itemSelectionPanel.activeSelf)
            OpenFoodSelection();
    }

    private GameObject FindGearUpSlot(InventoryItemData data)
    {
        if (gearUpFoodContent == null || data == null) return null;
        string id = SafeName(data.itemID);
        for (int i = 0; i < gearUpFoodContent.childCount; i++)
        {
            var child = gearUpFoodContent.GetChild(i).gameObject;
            if (child != null && child.name.StartsWith("GearUpFood_" + id, StringComparison.Ordinal))
                return child;
        }
        return null;
    }

    private void CloseFoodSelection()
    {
        itemSelectionPanel?.SetActive(false);
    }

    // Wire RemoveButton if present; fallback to slot click if not
    private void WireRemoveButton(Transform slotRoot, InventoryItemData itemData)
    {
        // Show RemoveButton in Gear Up, hide DropButton
        ShowButton(slotRoot, "DropButton", false);

        var removeTf = slotRoot.Find("RemoveButton");
        Button removeBtn = null;
        if (removeTf != null) removeBtn = removeTf.GetComponent<Button>();

        if (removeBtn != null)
        {
            removeTf.gameObject.SetActive(true);
            removeBtn.onClick.RemoveAllListeners();
            removeBtn.onClick.AddListener(() => RemoveFoodFromGearUp(itemData, 1));
        }
        else
        {
            // Fall back: clicking the slot removes one
            var slotBtn = slotRoot.GetComponent<Button>();
            if (slotBtn != null)
            {
                slotBtn.onClick.RemoveAllListeners();
                slotBtn.onClick.AddListener(() => RemoveFoodFromGearUp(itemData, 1));
            }
        }
    }

    // Utility: show/hide a named child button in a slot
    private void ShowButton(Transform root, string childName, bool visible)
    {
        if (root == null || string.IsNullOrEmpty(childName)) return;
        var tf = root.Find(childName);
        if (tf != null && tf.gameObject.activeSelf != visible)
            tf.gameObject.SetActive(visible);
    }

    // Sets icon, name, quantity, and rarity color on a food slot
    private void SetupFoodSlotUI(Transform slotRoot, InventoryItemData data, int quantity)
    {
        if (slotRoot == null || data == null) return;

        // Icon
        var iconTf = slotRoot.Find("ItemInfo/Icon");
        if (iconTf != null)
        {
            var iconImg = iconTf.GetComponent<Image>();
            if (iconImg != null)
            {
                iconImg.sprite = data.icon;
                iconImg.preserveAspect = true;
                iconImg.color = Color.white;
            }
        }

        // Name
        TextMeshProUGUI nameText = null;
        var nameTf = slotRoot.Find("ItemInfo/NameText");
        if (nameTf != null) nameText = nameTf.GetComponent<TextMeshProUGUI>();
        if (nameText == null)
        {
            var itemInfo = slotRoot.Find("ItemInfo");
            if (itemInfo != null)
            {
                var tmps = itemInfo.GetComponentsInChildren<TextMeshProUGUI>(true);
                for (int i = 0; i < tmps.Length; i++)
                {
                    if (tmps[i].name.IndexOf("Name", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        nameText = tmps[i];
                        break;
                    }
                }
                if (nameText == null && tmps.Length > 0) nameText = tmps[0];
            }
        }
        if (nameText != null) nameText.text = data.itemName;

        // Quantity text
        UpdateFoodSlotQuantity(slotRoot, quantity);

        // Rarity color
        ApplyRarityColor(slotRoot, data.rarity);

        // Normalize transform basics
        var rt = slotRoot.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
        }

        // Ensure Gear Up button visibility logic (Drop off, Remove on)
        ShowButton(slotRoot, "DropButton", false);
        var removeTf = slotRoot.Find("RemoveButton");
        if (removeTf != null) removeTf.gameObject.SetActive(true);
    }

    private void UpdateFoodSlotQuantity(Transform slotRoot, int quantity)
    {
        if (slotRoot == null) return;

        TextMeshProUGUI qtyText = null;
        var qtyTf = slotRoot.Find("ItemInfo/QuantityText");
        if (qtyTf != null) qtyText = qtyTf.GetComponent<TextMeshProUGUI>();
        if (qtyText == null)
        {
            var itemInfo = slotRoot.Find("ItemInfo");
            if (itemInfo != null)
            {
                var tmps = itemInfo.GetComponentsInChildren<TextMeshProUGUI>(true);
                for (int i = 0; i < tmps.Length; i++)
                {
                    if (tmps[i].name.IndexOf("Quantity", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        qtyText = tmps[i];
                        break;
                    }
                }
            }
        }
        if (qtyText != null) qtyText.text = "x" + Mathf.Max(1, quantity).ToString();
    }

    private void ApplyRarityColor(Transform slot, ItemRarity rarity)
    {
        var raritySlider = slot.GetComponentInChildren<Slider>(true);
        if (raritySlider != null && raritySlider.fillRect != null)
        {
            var fillImage = raritySlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                var fillColor = RarityColors.GetColor(rarity);
                fillImage.color = fillColor;

                var all = raritySlider.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] != fillImage)
                        all[i].color = DarkenColor(fillImage.color, 0.75f);
                }
            }
        }
    }

    private static string SafeName(string s)
    {
        if (string.IsNullOrEmpty(s)) return "NULL";
        return s.Replace(" ", "");
    }

    private Color DarkenColor(Color color, float factor)
    {
        return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
    }
}
