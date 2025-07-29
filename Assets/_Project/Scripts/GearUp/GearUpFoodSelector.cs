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

        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(true);

        if (selectionTitle != null)
            selectionTitle.text = "Select Food";

        foreach (Transform child in itemScrollViewContent)
        {
            Destroy(child.gameObject);
        }

        Dictionary<InventoryItemData, int> stashItems = StashManager.Instance != null ? StashManager.Instance.stashItems : null;
        if (stashItems == null)
            return;

        foreach (KeyValuePair<InventoryItemData, int> entry in stashItems)
        {
            InventoryItemData itemData = entry.Key;
            int quantity = entry.Value;

            if (itemData == null)
                continue;

            if (itemData.category.Equals("Food", StringComparison.OrdinalIgnoreCase) && quantity > 0)
            {
                GameObject slotGO = Instantiate(foodSlotPrefab, itemScrollViewContent);
                slotGO.name = "FoodSlot_" + itemData.itemName + "_" + Guid.NewGuid().ToString("N");

                RectTransform rt = slotGO.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition3D = Vector3.zero;
                }

                Transform itemInfo = slotGO.transform.Find("ItemInfo");
                if (itemInfo != null)
                {
                    Transform iconTransform = itemInfo.Find("Icon");
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

        CloseFoodSelection();
    }

    void CloseFoodSelection()
    {
        if (itemSelectionPanel != null)
        {
            itemSelectionPanel.SetActive(false);
            Debug.Log("Closed food selection.");
        }
    }

    private void ApplyRarityColor(Transform slot, ItemRarity rarity)
    {
        Slider raritySlider = slot.GetComponentInChildren<Slider>();
        if (raritySlider != null && raritySlider.fillRect != null)
        {
            Image fillImage = raritySlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = GetColorForRarity(rarity);
            }
        }
    }

    private Color GetColorForRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Uncommon: return Color.green;
            case ItemRarity.Rare: return Color.cyan;
            case ItemRarity.Epic: return new Color(0.5f, 0f, 1f); // Purple
            case ItemRarity.Legendary: return new Color(1f, 0.5f, 0f); // Orange
            default: return Color.gray;
        }
    }
}
