using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class GearUpWeaponSelector : MonoBehaviour
{
    [Header("UI References")]
    public Button weaponSlotButton;
    public Button backButton;
    public GameObject itemSelectionPanel;
    public TextMeshProUGUI selectionTitle;
    public Transform itemScrollViewContent;
    public GameObject weaponSlotPrefab;

    private void Start()
    {
        weaponSlotButton.onClick.AddListener(OpenWeaponSelection);

        if (backButton != null)
            backButton.onClick.AddListener(CloseWeaponSelection);

        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(false);
    }

    void OpenWeaponSelection()
    {
        Debug.Log("Opening weapon selection...");

        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(true);

        if (selectionTitle != null)
            selectionTitle.text = "Select a Weapon";

        foreach (Transform child in itemScrollViewContent)
            Destroy(child.gameObject);

        List<ItemInstance> stashItems = StashManager.Instance != null ? StashManager.Instance.stashInstances : null;
        if (stashItems == null)
            return;

        foreach (ItemInstance item in stashItems)
        {
            if (item.itemData != null &&
                item.itemData.category.Equals("Weapon", StringComparison.OrdinalIgnoreCase))
            {
                CreateWeaponSlot(item, item.quantity, item.currentDurability);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemScrollViewContent.GetComponent<RectTransform>());
    }

    void CreateWeaponSlot(ItemInstance instance, int quantity, int currentDurability = -1)
    {
        if (instance == null || instance.itemData == null) return;

        InventoryItemData itemData = instance.itemData;

        GameObject slotGO = Instantiate(weaponSlotPrefab, itemScrollViewContent);
        slotGO.name = "WeaponSlot_" + itemData.itemName + "_" + Guid.NewGuid().ToString("N");

        RectTransform rt = slotGO.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
        }

        InventoryItemUI ui = slotGO.GetComponent<InventoryItemUI>();
        if (ui != null)
        {
            int durabilityToUse = itemData.isDurable ? currentDurability : -1;
            ui.Setup(itemData, quantity, durabilityToUse);
        }

        ApplyRarityColorToSlider(slotGO.transform, itemData.rarity);

        Button button = slotGO.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => AssignWeapon(instance));
        }
    }

    void AssignWeapon(ItemInstance selectedInstance)
    {
        if (selectedInstance == null || selectedInstance.itemData == null)
        {
            Debug.LogWarning("AssignWeapon: selectedInstance or itemData is null.");
            return;
        }

        InventoryItemData selectedItem = selectedInstance.itemData;
        Debug.Log("Selected weapon: " + selectedItem.itemName);

        if (weaponSlotButton != null)
        {
            Image buttonImage = weaponSlotButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = null;
                buttonImage.color = RarityColors.GetColor(selectedItem.rarity);
            }

            Transform iconTransform = weaponSlotButton.transform.Find("WeaponSlotButtonBackground");
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

        //  Track weapon for exploration
        GearUpSelectionManager.Instance?.AddDurable(selectedInstance);

        CloseWeaponSelection();
    }

    void CloseWeaponSelection()
    {
        if (itemSelectionPanel != null)
        {
            itemSelectionPanel.SetActive(false);
            Debug.Log("Closed weapon selection.");
        }
    }

    private void ApplyRarityColorToSlider(Transform slot, ItemRarity rarity)
    {
        Slider durabilitySlider = slot.GetComponentInChildren<Slider>();
        if (durabilitySlider != null)
        {
            Color fillColor = RarityColors.GetColor(rarity);
            Color backgroundColor = DarkenColor(fillColor, 0.75f);

            Transform fillTransform = durabilitySlider.transform.Find("Fill Area/Fill");
            if (fillTransform != null)
            {
                Image fillImage = fillTransform.GetComponent<Image>();
                if (fillImage != null)
                    fillImage.color = fillColor;
            }

            Transform bgTransform = durabilitySlider.transform.Find("Background");
            if (bgTransform != null)
            {
                Image bgImage = bgTransform.GetComponent<Image>();
                if (bgImage != null)
                    bgImage.color = backgroundColor;
            }
        }
    }

    private Color DarkenColor(Color color, float amount = 0.75f)
    {
        return new Color(color.r * amount, color.g * amount, color.b * amount, color.a);
    }
}
