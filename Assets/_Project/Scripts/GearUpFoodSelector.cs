using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class GearUpFoodSelector : MonoBehaviour
{
    [Header("UI References")]
    public Button addFoodButton;
    public Button backButton;
    public GameObject itemSelectionPanel;
    public TextMeshProUGUI selectionTitle;
    public Transform itemScrollViewContent; // the scroll area inside ItemSelectionPanel
    public GameObject foodSlotPrefab; // the visual slot prefab shown in selection panel
    public Transform gearUpFoodContent; // target container: GearUpPanel > Scroll View > Viewport > Content

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

        List<InventoryEntry> inventory = InventoryManager.Instance.inventory;
        Debug.Log("Inventory Count: " + inventory.Count);

        foreach (InventoryEntry entry in inventory)
        {
            if (entry.itemData == null)
            {
                Debug.LogWarning("Inventory entry has null itemData.");
                continue;
            }

            if (entry.itemData.category.Equals("Food", StringComparison.OrdinalIgnoreCase) && entry.quantity > 0)
            {
                GameObject slotGO = Instantiate(foodSlotPrefab, itemScrollViewContent);
                slotGO.name = "FoodSlot_" + entry.itemData.itemName + "_" + Guid.NewGuid().ToString("N");

                RectTransform rt = slotGO.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.one;
                    rt.anchoredPosition3D = Vector3.zero;
                }

                // Setup UI elements like icon, name, etc.
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
                        }
                    }
                }

                // Assign button click to add to GearUp panel
                Button button = slotGO.GetComponent<Button>();
                if (button != null)
                {
                    InventoryItemData capturedItem = entry.itemData;
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
}
