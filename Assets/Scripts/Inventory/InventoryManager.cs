using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject itemIconPrefab;
    [SerializeField] private Transform dropPoint;

    private InventorySlotUI[] slots;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (slotsParent != null)
            slots = slotsParent.GetComponentsInChildren<InventorySlotUI>();

        RefreshUI();
    }

    public void AddItem(InventoryItem newItem)
    {
        if (newItem == null)
            return;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] = newItem;
                RefreshUI();
                return;
            }
        }

        items.Add(newItem);
        RefreshUI();
    }

    public List<InventoryItem> GetItems()
    {
        return items;
    }

    public InventoryItem GetItemAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Count)
            return null;

        return items[slotIndex];
    }

    public int GetSlotIndex(InventorySlotUI slot)
    {
        if (slots == null || slot == null)
            return -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == slot)
                return i;
        }

        return -1;
    }

    public bool MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || toIndex < 0)
            return false;

        if (slots == null || toIndex >= slots.Length)
            return false;

        while (items.Count <= fromIndex)
            items.Add(null);

        while (items.Count <= toIndex)
            items.Add(null);

        if (items[fromIndex] == null)
            return false;

        if (fromIndex == toIndex)
        {
            RefreshUI();
            return true;
        }

        InventoryItem temp = items[toIndex];
        items[toIndex] = items[fromIndex];
        items[fromIndex] = temp;

        RefreshUI();
        return true;
    }

    public bool DropItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0)
            return false;

        while (items.Count <= slotIndex)
            items.Add(null);

        InventoryItem item = items[slotIndex];
        if (item == null)
            return false;

        GameObject prefabToDrop = item.itemObject != null ? item.itemObject : item.equippedObject;

        if (prefabToDrop == null)
        {
            Debug.LogWarning("No itemObject or equippedObject assigned for: " + item.itemName);
            return false;
        }

        Vector3 dropPosition;
        if (dropPoint != null)
            dropPosition = dropPoint.position;
        else if (Camera.main != null)
            dropPosition = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        else
            dropPosition = transform.position + transform.forward * 2f;

        GameObject droppedObject = Instantiate(prefabToDrop, dropPosition, Quaternion.identity);
        droppedObject.SetActive(true);

        Collider col = droppedObject.GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        WorldItemPickup pickup = droppedObject.GetComponent<WorldItemPickup>();
        if (pickup == null)
            pickup = droppedObject.AddComponent<WorldItemPickup>();

        pickup.SetItem(item);

        items[slotIndex] = null;
        RefreshUI();
        return true;
    }

    public void RefreshUI()
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearSlot();

            if (i < items.Count && items[i] != null)
                slots[i].SetItem(items[i], itemIconPrefab);
        }
    }
}