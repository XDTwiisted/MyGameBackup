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

        List<ItemInstance> stashItems = StashManager.Instance != null ? StashManager.Instance.stashInstances : null;
        if (stashItems == null)
            return;

        foreach (ItemInstance item in stashItems)
        {
            if (item.itemData == null)
                continue;

            if (item.itemData.category.Equals("Tool", StringComparison.OrdinalIgnoreCase))
            {
                GameObject slotGO = Instantiate(toolSlotPrefab, itemScrollViewContent);
                slotGO.name = "ToolSlot_" + item.itemData.itemName + "_" + Guid.NewGuid().ToString("N");

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
                            iconImage.sprite = item.itemData.icon;
                            iconImage.preserveAspect = true;
                            iconImage.color = Color.white;
                        }
                    }
                }

                ApplyRarityVisualsToSlider(slotGO.transform, item);

                Button button = slotGO.GetComponent<Button>();
                if (button != null)
                {
                    ItemInstance capturedInstance = item;
                    button.onClick.AddListener(() => AssignTool(capturedInstance));
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemScrollViewContent.GetComponent<RectTransform>());
    }

    void AssignTool(ItemInstance selectedInstance)
    {
        if (selectedInstance == null || selectedInstance.itemData == null)
        {
            Debug.LogWarning("AssignTool: selectedInstance or itemData is null.");
            return;
        }

        Debug.Log("Selected tool: " + selectedInstance.itemData.itemName);

        if (toolSlotButton != null)
        {
            Image buttonImage = toolSlotButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = null;
                buttonImage.color = RarityColors.GetColor(selectedInstance.itemData.rarity);
            }

            Transform iconTransform = toolSlotButton.transform.Find("ToolSlotButtonBackground");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = selectedInstance.itemData.icon;
                    iconImage.preserveAspect = true;
                    iconImage.color = Color.white;
                }
            }
        }

        GearUpSelectionManager.Instance?.AddDurable(selectedInstance);
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

    private void ApplyRarityVisualsToSlider(Transform slot, ItemInstance item)
    {
        if (item == null || item.itemData == null) return;

        Slider durabilitySlider = slot.GetComponentInChildren<Slider>();
        if (durabilitySlider == null) return;

        durabilitySlider.maxValue = item.itemData.maxDurability;
        durabilitySlider.value = item.currentDurability;

        Color fillColor = RarityColors.GetColor(item.itemData.rarity);
        Color backgroundColor = DarkenColor(fillColor, 0.5f);

        Transform fillTransform = durabilitySlider.transform.Find("Fill Area/Fill");
        if (fillTransform != null)
        {
            Image fillImage = fillTransform.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = fillColor;
            }
        }

        Transform bgTransform = durabilitySlider.transform.Find("Background");
        if (bgTransform != null)
        {
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = backgroundColor;
            }
        }
    }

    private Color DarkenColor(Color color, float multiplier)
    {
        return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
    }
}
