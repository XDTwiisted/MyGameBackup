using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StashIconDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public InventoryItemData itemData;   // set at runtime
    public Image iconImage;              // auto-found if left empty
    public Canvas parentCanvas;          // auto-found if left empty

    private CanvasGroup cg;
    private RectTransform dragRT;
    private Image dragGhost;
    private static StashIconDrag currentDragging;

    void Awake()
    {
        if (iconImage == null)
        {
            var t = transform.Find("Icon");
            if (t == null) t = transform.Find("StashIconItem");
            iconImage = t ? t.GetComponent<Image>() : GetComponentInChildren<Image>();
        }
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemData == null || iconImage == null || iconImage.sprite == null) return;
        if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;

        currentDragging = this;
        cg.blocksRaycasts = false;

        GameObject ghost = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        ghost.transform.SetParent(parentCanvas.transform, false);
        dragRT = ghost.GetComponent<RectTransform>();
        dragRT.sizeDelta = iconImage.rectTransform.rect.size;

        dragGhost = ghost.GetComponent<Image>();
        dragGhost.sprite = iconImage.sprite;
        dragGhost.preserveAspect = true;
        dragGhost.raycastTarget = false;

        UpdateGhost(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragRT != null) UpdateGhost(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragRT != null) Destroy(dragRT.gameObject);
        dragRT = null;
        dragGhost = null;
        if (cg != null) cg.blocksRaycasts = true;
        currentDragging = null;
    }

    private void UpdateGhost(PointerEventData evt)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            evt.position,
            evt.pressEventCamera,
            out var localPos
        );
        dragRT.anchoredPosition = localPos;
    }

    public static bool TryGetCurrent(out StashIconDrag drag)
    {
        drag = currentDragging;
        return drag != null && drag.itemData != null;
    }
}
