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

    // Track the tool currently assigned to the Gear Up slot
    private ItemInstance currentAssignedTool = null;

    // Track tools selected during this session so they do not show in the list
    private HashSet<ItemInstance> selectedTools = new HashSet<ItemInstance>();

    private void Start()
    {
        if (toolSlotButton != null) toolSlotButton.onClick.AddListener(OpenToolSelection);
        if (backButton != null) backButton.onClick.AddListener(CloseToolSelection);
        if (itemSelectionPanel != null) itemSelectionPanel.SetActive(false);
    }

    private void OpenToolSelection()
    {
        if (itemSelectionPanel != null) itemSelectionPanel.SetActive(true);
        if (selectionTitle != null) selectionTitle.text = "Select a Tool";

        if (itemScrollViewContent != null)
        {
            for (int i = itemScrollViewContent.childCount - 1; i >= 0; i--)
                Destroy(itemScrollViewContent.GetChild(i).gameObject);
        }

        List<ItemInstance> stashItems = (StashManager.Instance != null) ? StashManager.Instance.stashInstances : null;
        if (stashItems == null) return;

        foreach (ItemInstance item in stashItems)
        {
            if (item == null || item.itemData == null) continue;
            if (!item.itemData.category.Equals("Tool", StringComparison.OrdinalIgnoreCase)) continue;

            // Hide items currently marked as selected (including the currentAssignedTool).
            if (selectedTools.Contains(item)) continue;

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

                Transform nameTf = itemInfo.Find("NameText");
                if (nameTf != null)
                {
                    var nameTMP = nameTf.GetComponent<TextMeshProUGUI>();
                    if (nameTMP != null) nameTMP.text = item.itemData.itemName;
                }
            }

            ApplyRarityVisualsToSlider(slotGO.transform, item);

            Button button = slotGO.GetComponent<Button>();
            if (button != null)
            {
                ItemInstance captured = item;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => AssignTool(captured));
            }
        }

        var contentRT = (itemScrollViewContent != null) ? itemScrollViewContent.GetComponent<RectTransform>() : null;
        if (contentRT != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
    }

    private void AssignTool(ItemInstance selectedInstance)
    {
        if (selectedInstance == null || selectedInstance.itemData == null)
        {
            Debug.LogWarning("AssignTool: selectedInstance or itemData is null.");
            return;
        }

        // If we already had a tool assigned and it is different, unselect it so it appears again in the list
        if (currentAssignedTool != null && currentAssignedTool != selectedInstance)
        {
            selectedTools.Remove(currentAssignedTool);

            // Remove previous from GearUp selections so Confirm will not carry it
            var gsmPrev = GearUpSelectionManager.Instance;
            if (gsmPrev != null) gsmPrev.RemoveDurable(currentAssignedTool);
        }

        // Assign the new tool to the Gear Up button visuals
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

        // Mark new selection and register with GearUp so it transfers on Confirm
        selectedTools.Add(selectedInstance);
        currentAssignedTool = selectedInstance;

        var gsm = GearUpSelectionManager.Instance;
        if (gsm != null)
        {
            // Ensure only this tool is in the GearUp durables list
            gsm.RemoveDurable(selectedInstance); // no-op if not present
            if (currentAssignedTool != null && currentAssignedTool != selectedInstance)
                gsm.RemoveDurable(currentAssignedTool); // handled above, but safe

            gsm.AddDurable(selectedInstance);
        }

        CloseToolSelection();
    }

    private void CloseToolSelection()
    {
        if (itemSelectionPanel != null) itemSelectionPanel.SetActive(false);
    }

    private void ApplyRarityVisualsToSlider(Transform slot, ItemInstance item)
    {
        if (slot == null || item == null || item.itemData == null) return;

        Slider durabilitySlider = slot.GetComponentInChildren<Slider>(true);
        if (durabilitySlider == null) return;

        durabilitySlider.maxValue = item.itemData.maxDurability;
        durabilitySlider.value = item.currentDurability;

        Color fillColor = RarityColors.GetColor(item.itemData.rarity);
        Color backgroundColor = DarkenColor(fillColor, 0.5f);

        Transform fillTransform = durabilitySlider.transform.Find("Fill Area/Fill");
        if (fillTransform != null)
        {
            Image fillImage = fillTransform.GetComponent<Image>();
            if (fillImage != null) fillImage.color = fillColor;
        }

        Transform bgTransform = durabilitySlider.transform.Find("Background");
        if (bgTransform != null)
        {
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null) bgImage.color = backgroundColor;
        }
    }

    private Color DarkenColor(Color color, float multiplier)
    {
        return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
    }
}
