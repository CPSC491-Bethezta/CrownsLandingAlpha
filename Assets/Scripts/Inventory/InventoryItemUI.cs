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

        if (iconImage == null)
            iconImage = GetComponent<Image>();

        if (iconImage != null && item != null)
            iconImage.sprite = item.icon;
    }

    private void Awake()
    {
        iconImage = GetComponent<Image>();
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
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        InventorySlotUI targetSlot = null;

        if (eventData.pointerEnter != null)
            targetSlot = eventData.pointerEnter.GetComponentInParent<InventorySlotUI>();

        // Dropped on a valid slot — move or swap
        if (targetSlot != null)
        {
            int fromIndex = InventoryManager.Instance.GetSlotIndex(originalSlot);
            int toIndex = InventoryManager.Instance.GetSlotIndex(targetSlot);

            bool moved = InventoryManager.Instance.MoveItem(fromIndex, toIndex);

            if (moved)
                return;
        }

        // Dropped outside all UI elements — drop item into the world
        if (eventData.pointerEnter == null)
        {
            int fromIndex = InventoryManager.Instance.GetSlotIndex(originalSlot);
            bool dropped = InventoryManager.Instance.DropItemFromSlot(fromIndex);

            if (dropped)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Fallback — snap back to original slot
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}