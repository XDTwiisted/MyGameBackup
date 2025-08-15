using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoodDropSlot : MonoBehaviour, IDropHandler
{
    public Image targetImage;        // assign this in Inspector (the Image you are dropping onto)
    public InventoryItemData currentItem;
    public bool onlyAcceptFood = true;

    void Reset()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!StashIconDrag.TryGetCurrent(out var drag)) return;
        if (drag.itemData == null || drag.iconImage == null || drag.iconImage.sprite == null) return;

        if (onlyAcceptFood)
        {
            string cat = (drag.itemData.category ?? "").ToLowerInvariant();
            if (!cat.Contains("food")) return;
        }

        // Set this slot's image to the dropped sprite
        if (targetImage != null)
        {
            targetImage.sprite = drag.iconImage.sprite;
            targetImage.preserveAspect = true;
            targetImage.enabled = true;
        }

        // Remember which item was dropped
        currentItem = drag.itemData;

        // Optional: to remove one from stash immediately, uncomment:
        // StashManager.Instance.RemoveStackableItem(currentItem, 1);
    }
}
