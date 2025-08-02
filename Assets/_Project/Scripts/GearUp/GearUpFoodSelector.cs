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

    private HashSet<InventoryItemData> selectedItems = new HashSet<InventoryItemData>();

    private void Start()
    {
        addFoodButton.onClick.AddListener(OpenFoodSelection);

        if (backButton != null)
            backButton.onClick.AddListener(CloseFoodSelection);

        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(false);
    }

    void OpenFoodSelection()
    {
        Debug.Log("Opening food selection...");

        itemSelectionPanel?.SetActive(true);
        selectionTitle.text = "Select Food";

        foreach (Transform child in itemScrollViewContent)
            Destroy(child.gameObject);

        Dictionary<InventoryItemData, int> stashItems = StashManager.Instance?.stashItems;
        if (stashItems == null)
            return;

        foreach (KeyValuePair<InventoryItemData, int> entry in stashItems)
        {
            InventoryItemData itemData = entry.Key;
            int quantity = entry.Value;

            if (itemData == null || quantity <= 0)
                continue;

            if (itemData.category.Equals("Food", StringComparison.OrdinalIgnoreCase) &&
                !selectedItems.Contains(itemData))
            {
                GameObject slotGO = Instantiate(foodSlotPrefab, itemScrollViewContent);
                slotGO.name = "FoodSlot_" + itemData.itemName + "_" + Guid.NewGuid().ToString("N");

                RectTransform rt = slotGO.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition3D = Vector3.zero;
                }

                Transform itemInfo = slotGO.transform.Find("ItemInfo/Icon");
                if (itemInfo != null)
                {
                    Image iconImage = itemInfo.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = itemData.icon;
                        iconImage.preserveAspect = true;
                        iconImage.color = Color.white;
                    }
                }

                ApplyRarityColor(slotGO.transform, itemData.rarity);

                Button button = slotGO.GetComponent<Button>();
                if (button != null)
                {
                    InventoryItemData capturedItem = itemData;
                    button.onClick.AddListener(() => AddFoodToGearUp(capturedItem));
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemScrollViewContent.GetComponent<RectTransform>());
    }

    void AddFoodToGearUp(InventoryItemData itemData)
    {
        Debug.Log("Adding food to GearUp panel: " + itemData.itemName);

        GameObject newFoodSlot = Instantiate(foodSlotPrefab, gearUpFoodContent);
        newFoodSlot.name = "GearUpFood_" + itemData.itemName + "_" + Guid.NewGuid().ToString("N");

        RectTransform rt = newFoodSlot.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
        }

        Transform iconTransform = newFoodSlot.transform.Find("ItemInfo/Icon");
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = itemData.icon;
                iconImage.preserveAspect = true;
                iconImage.color = Color.white;
            }
        }

        ApplyRarityColor(newFoodSlot.transform, itemData.rarity);

        // Track this item so it doesn't show up again
        selectedItems.Add(itemData);

        // Subtract from stash
        if (StashManager.Instance != null)
        {
            if (StashManager.Instance.stashItems.ContainsKey(itemData))
            {
                StashManager.Instance.stashItems[itemData] -= 1;
                if (StashManager.Instance.stashItems[itemData] <= 0)
                    StashManager.Instance.stashItems.Remove(itemData);
            }
        }

        CloseFoodSelection();
    }

    void CloseFoodSelection()
    {
        itemSelectionPanel?.SetActive(false);
        Debug.Log("Closed food selection.");
    }

    private void ApplyRarityColor(Transform slot, ItemRarity rarity)
    {
        Slider raritySlider = slot.GetComponentInChildren<Slider>();
        if (raritySlider != null && raritySlider.fillRect != null)
        {
            Image fillImage = raritySlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                Color fillColor = GetColorForRarity(rarity);
                fillImage.color = fillColor;

                foreach (Image img in raritySlider.GetComponentsInChildren<Image>())
                {
                    if (img != fillImage)
                        img.color = DarkenColor(fillColor, 0.75f);
                }
            }
        }
    }

    private Color GetColorForRarity(ItemRarity rarity)
    {
        return RarityColors.GetColor(rarity);
    }

    private Color DarkenColor(Color color, float factor)
    {
        return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
    }
}
