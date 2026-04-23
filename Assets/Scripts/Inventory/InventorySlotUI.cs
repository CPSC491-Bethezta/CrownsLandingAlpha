using UnityEngine;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Equipment Slot Settings")]
    [SerializeField] private bool isEquipmentSlot;
    [SerializeField] private InventoryItemType acceptedItemType = InventoryItemType.General;

    private InventoryItem currentItem;
    private GameObject currentIconObject;

    public bool IsEquipmentSlot()
    {
        return isEquipmentSlot;
    }

    public InventoryItemType GetAcceptedItemType()
    {
        return acceptedItemType;
    }

    public void SetItem(InventoryItem item, GameObject itemIconPrefab)
    {
        currentItem = item;

        if (item == null || itemIconPrefab == null)
            return;

        currentIconObject = Instantiate(itemIconPrefab, transform);

        RectTransform rt = currentIconObject.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        InventoryItemUI itemUI = currentIconObject.GetComponent<InventoryItemUI>();
        if (itemUI != null)
            itemUI.Setup(item, this);
    }

    public void ClearSlot()
    {
        currentItem = null;

        if (currentIconObject != null)
            Destroy(currentIconObject);

        currentIconObject = null;
    }

    public bool IsEmpty()
    {
        return currentItem == null;
    }
}