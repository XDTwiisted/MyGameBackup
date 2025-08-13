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

    // Track the weapon currently assigned to the Gear Up slot
    private ItemInstance currentAssignedWeapon = null;

    // Track weapons selected during this session so they do not show in the list
    private HashSet<ItemInstance> selectedWeapons = new HashSet<ItemInstance>();

    private void Start()
    {
        if (weaponSlotButton != null) weaponSlotButton.onClick.AddListener(OpenWeaponSelection);
        if (backButton != null) backButton.onClick.AddListener(CloseWeaponSelection);
        if (itemSelectionPanel != null) itemSelectionPanel.SetActive(false);
    }

    private void OpenWeaponSelection()
    {
        if (itemSelectionPanel != null) itemSelectionPanel.SetActive(true);
        if (selectionTitle != null) selectionTitle.text = "Select a Weapon";

        // Clear previous entries
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
            if (!item.itemData.category.Equals("Weapon", StringComparison.OrdinalIgnoreCase)) continue;

            // Hide items currently marked as selected (including the currentAssignedWeapon)
            if (selectedWeapons.Contains(item)) continue;

            CreateWeaponSlot(item);
        }

        var contentRT = (itemScrollViewContent != null) ? itemScrollViewContent.GetComponent<RectTransform>() : null;
        if (contentRT != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
    }

    private void CreateWeaponSlot(ItemInstance instance)
    {
        var data = instance.itemData;

        GameObject slotGO = Instantiate(weaponSlotPrefab, itemScrollViewContent);
        slotGO.name = "WeaponSlot_" + data.itemName + "_" + Guid.NewGuid().ToString("N");

        RectTransform rt = slotGO.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
        }

        // If your prefab uses InventoryItemUI, set it up
        var ui = slotGO.GetComponent<InventoryItemUI>();
        if (ui != null)
        {
            int dur = data.isDurable ? instance.currentDurability : -1;
            ui.Setup(data, instance.quantity, dur);
        }
        else
        {
            // Fallback: set icon and name manually if needed
            Transform itemInfo = slotGO.transform.Find("ItemInfo");
            if (itemInfo != null)
            {
                var iconTf = itemInfo.Find("Icon");
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

                var nameTf = itemInfo.Find("NameText");
                if (nameTf != null)
                {
                    var nameTMP = nameTf.GetComponent<TextMeshProUGUI>();
                    if (nameTMP != null) nameTMP.text = data.itemName;
                }
            }

            ApplyRarityColorToSlider(slotGO.transform, data.rarity);
        }

        Button button = slotGO.GetComponent<Button>();
        if (button != null)
        {
            ItemInstance captured = instance; // local capture avoids closure issues
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => AssignWeapon(captured));
        }
    }

    private void AssignWeapon(ItemInstance selectedInstance)
    {
        if (selectedInstance == null || selectedInstance.itemData == null)
        {
            Debug.LogWarning("AssignWeapon: selectedInstance or itemData is null.");
            return;
        }

        // If a different weapon was already assigned, unselect it so it appears again next time
        if (currentAssignedWeapon != null && currentAssignedWeapon != selectedInstance)
        {
            selectedWeapons.Remove(currentAssignedWeapon);

            // Remove previous from gear-up selections so Confirm will not carry it
            var gsmPrev = GearUpSelectionManager.Instance;
            if (gsmPrev != null) gsmPrev.RemoveDurable(currentAssignedWeapon);
        }

        // Update the Gear Up slot button visuals
        var data = selectedInstance.itemData;
        if (weaponSlotButton != null)
        {
            Image buttonImage = weaponSlotButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = null;
                buttonImage.color = RarityColors.GetColor(data.rarity);
            }

            Transform iconTransform = weaponSlotButton.transform.Find("WeaponSlotButtonBackground");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = data.icon;
                    iconImage.preserveAspect = true;
                    iconImage.color = Color.white;
                }
            }
        }

        // Mark new selection and register so it transfers on Confirm
        selectedWeapons.Add(selectedInstance);
        currentAssignedWeapon = selectedInstance;

        var gsm = GearUpSelectionManager.Instance;
        if (gsm != null)
        {
            // Ensure only this weapon is in the gear-up durables list
            gsm.RemoveDurable(selectedInstance); // harmless if not present
            if (currentAssignedWeapon != null && currentAssignedWeapon != selectedInstance)
                gsm.RemoveDurable(currentAssignedWeapon); // already handled above, but safe

            gsm.AddDurable(selectedInstance);
        }

        CloseWeaponSelection();
    }

    private void CloseWeaponSelection()
    {
        if (itemSelectionPanel != null)
        {
            itemSelectionPanel.SetActive(false);
        }
    }

    private void ApplyRarityColorToSlider(Transform slot, ItemRarity rarity)
    {
        Slider durabilitySlider = slot.GetComponentInChildren<Slider>(true);
        if (durabilitySlider == null) return;

        Color fillColor = RarityColors.GetColor(rarity);
        Color backgroundColor = DarkenColor(fillColor, 0.75f);

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

    private Color DarkenColor(Color color, float amount = 0.75f)
    {
        return new Color(color.r * amount, color.g * amount, color.b * amount, color.a);
    }
}
