using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class GearUpToolSelector : MonoBehaviour
{
    [Header("UI References")]
    public Button toolSlotButton;
    public Button backButton;
    public GameObject itemSelectionPanel;
    public TextMeshProUGUI selectionTitle;
    public Transform itemScrollViewContent;
    public GameObject toolSlotPrefab;

    private void Start()
    {
        toolSlotButton.onClick.AddListener(OpenToolSelection);

        if (backButton != null)
            backButton.onClick.AddListener(CloseToolSelection);

        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(false);
    }

    void OpenToolSelection()
    {
        Debug.Log("Opening tool selection...");

        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(true);

        if (selectionTitle != null)
            selectionTitle.text = "Select a Tool";

        foreach (Transform child in itemScrollViewContent)
        {
            Destroy(child.gameObject);
        }

        List<InventoryEntry> inventory = InventoryManager.Instance.inventory;
        Debug.Log("Inventory Count: " + inventory.Count);

        foreach (InventoryEntry entry in inventory)
        {
            if (entry.itemData == null)
            {
                Debug.LogWarning("Inventory entry has null itemData.");
                continue;
            }

            Debug.Log($"Checking item: {entry.itemData.itemName}, Category: {entry.itemData.category}, Quantity: {entry.quantity}");

            if (entry.itemData.category.Equals("Tool", StringComparison.OrdinalIgnoreCase) && entry.quantity > 0)
            {
                Debug.Log("Adding to selector: " + entry.itemData.itemName);

                GameObject slotGO = Instantiate(toolSlotPrefab, itemScrollViewContent);
                slotGO.name = "ToolSlot_" + entry.itemData.itemName + "_" + Guid.NewGuid().ToString("N");

                RectTransform rt = slotGO.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition3D = Vector3.zero;
                }

                Image slotImage = slotGO.GetComponent<Image>();
                if (slotImage != null)
                {
                    slotImage.color = UnityEngine.Random.ColorHSV();
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
                            iconImage.sprite = entry.itemData.icon;
                            iconImage.preserveAspect = true;
                            iconImage.color = Color.white;
                            Debug.Log("Assigned icon: " + entry.itemData.icon.name);
                        }
                    }
                }

                Slider durabilitySlider = slotGO.GetComponentInChildren<Slider>();
                if (durabilitySlider != null)
                {
                    Color rarityColor = GetRarityColor(entry.itemData.rarity);
                    Color backgroundColor = DarkenColor(rarityColor, 0.5f);

                    Transform bgTransform = durabilitySlider.transform.Find("Background");
                    if (bgTransform != null)
                    {
                        Image background = bgTransform.GetComponent<Image>();
                        if (background != null)
                            background.color = backgroundColor;
                    }

                    Transform fillTransform = durabilitySlider.transform.Find("Fill Area/Fill");
                    if (fillTransform != null)
                    {
                        Image fillImage = fillTransform.GetComponent<Image>();
                        if (fillImage != null)
                            fillImage.color = rarityColor;
                    }
                }

                Button button = slotGO.GetComponent<Button>();
                if (button != null)
                {
                    InventoryItemData capturedItem = entry.itemData;
                    button.onClick.AddListener(() => AssignTool(capturedItem));
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemScrollViewContent.GetComponent<RectTransform>());
    }

    void AssignTool(InventoryItemData selectedItem)
    {
        Debug.Log("Selected tool: " + selectedItem.itemName);

        if (toolSlotButton != null)
        {
            // Set background color for rarity on main button
            Image buttonImage = toolSlotButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = null;
                buttonImage.color = GetRarityColor(selectedItem.rarity);
            }

            // Set sprite on ToolSlotButtonBackground child
            Transform iconTransform = toolSlotButton.transform.Find("ToolSlotButtonBackground");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = selectedItem.icon;
                    iconImage.preserveAspect = true;
                    iconImage.color = Color.white;
                }
            }
        }

        CloseToolSelection();
    }

    void CloseToolSelection()
    {
        if (itemSelectionPanel != null)
        {
            itemSelectionPanel.SetActive(false);
            Debug.Log("Closed tool selection.");
        }
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return new Color(0.8f, 0.8f, 0.8f);
            case ItemRarity.Uncommon: return new Color(0.2f, 0.8f, 0.2f);
            case ItemRarity.Rare: return new Color(0.2f, 0.4f, 0.8f);
            case ItemRarity.Epic: return new Color(0.6f, 0.2f, 0.8f);
            case ItemRarity.Legendary: return new Color(0.9f, 0.6f, 0.1f);
            default: return Color.white;
        }
    }

    private Color DarkenColor(Color color, float amount = 0.5f)
    {
        return new Color(color.r * amount, color.g * amount, color.b * amount, color.a);
    }
}
