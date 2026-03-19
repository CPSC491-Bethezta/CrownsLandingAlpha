using UnityEngine;

public class InventorySlotUI : MonoBehaviour
{
    private InventoryItem currentItem;
    private GameObject currentIconObject;

    public void SetItem(InventoryItem item, GameObject itemIconPrefab)
    {
        currentItem = item;

        if (item == null || itemIconPrefab == null)
            return;

        currentIconObject = Instantiate(itemIconPrefab, transform);

        InventoryItemUI itemUI = currentIconObject.GetComponent<InventoryItemUI>();
        if (itemUI != null)
        {
            itemUI.Setup(item, this);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;

        if (currentIconObject != null)
            Destroy(currentIconObject);
    }

    public bool IsEmpty()
    {
        return currentItem == null;
    }
}
