using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image iconImage;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private InventorySlotUI originalSlot;
    private InventoryItem itemData;

    public void Setup(InventoryItem item, InventorySlotUI slot)
    {
        itemData = item;
        originalSlot = slot;

        if (iconImage != null && item != null)
            iconImage.sprite = item.icon;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(canvas.transform);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}
