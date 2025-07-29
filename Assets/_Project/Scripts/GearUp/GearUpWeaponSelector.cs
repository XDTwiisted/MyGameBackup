using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

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
                CreateWeaponSlot(item.itemData, item.quantity, item.currentDurability);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemScrollViewContent.GetComponent<RectTransform>());
    }

    void CreateWeaponSlot(InventoryItemData itemData, int quantity, int currentDurability = -1)
    {
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

        Button button = slotGO.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => AssignWeapon(itemData));
        }
    }

    void AssignWeapon(InventoryItemData selectedItem)
    {
        Debug.Log("Selected weapon: " + selectedItem.itemName);

        if (weaponSlotButton != null)
        {
            Image buttonImage = weaponSlotButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = null;
                buttonImage.color = GetRarityColor(selectedItem.rarity);
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
}
