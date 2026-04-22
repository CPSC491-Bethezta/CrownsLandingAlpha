using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();
    [SerializeField] private List<InventoryItem> equipmentItems = new List<InventoryItem>();

    [SerializeField] private Transform slotsParent;
    [SerializeField] private Transform equipmentSlotsParent;
    [SerializeField] private GameObject itemIconPrefab;
    [SerializeField] private Transform dropPoint;

    private InventorySlotUI[] slots;
    private InventorySlotUI[] equipmentSlots;

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

        if (equipmentSlotsParent != null)
            equipmentSlots = equipmentSlotsParent.GetComponentsInChildren<InventorySlotUI>();

        if (equipmentSlots != null)
        {
            while (equipmentItems.Count < equipmentSlots.Length)
                equipmentItems.Add(null);
        }

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
        if (slot == null)
            return -1;

        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == slot)
                    return i;
            }
        }

        if (equipmentSlots != null)
        {
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                if (equipmentSlots[i] == slot)
                    return (slots != null ? slots.Length : 0) + i;
            }
        }

        return -1;
    }

    private InventorySlotUI GetSlotByIndex(int index)
    {
        int normalSlotCount = slots != null ? slots.Length : 0;

        if (index < normalSlotCount)
            return slots[index];

        int equipmentIndex = index - normalSlotCount;

        if (equipmentSlots != null && equipmentIndex >= 0 && equipmentIndex < equipmentSlots.Length)
            return equipmentSlots[equipmentIndex];

        return null;
    }
    public InventoryItem GetEquipmentItemAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equipmentItems.Count)
            return null;

        return equipmentItems[slotIndex];
    }
    private InventoryItem GetItemByIndex(int index)
    {
        int normalSlotCount = slots != null ? slots.Length : 0;

        if (index < normalSlotCount)
        {
            if (index >= 0 && index < items.Count)
                return items[index];

            return null;
        }

        int equipmentIndex = index - normalSlotCount;

        if (equipmentIndex >= 0 && equipmentIndex < equipmentItems.Count)
            return equipmentItems[equipmentIndex];

        return null;
    }

    private void SetItemByIndex(int index, InventoryItem item)
    {
        int normalSlotCount = slots != null ? slots.Length : 0;

        if (index < normalSlotCount)
        {
            while (items.Count <= index)
                items.Add(null);

            items[index] = item;
            return;
        }

        int equipmentIndex = index - normalSlotCount;

        while (equipmentItems.Count <= equipmentIndex)
            equipmentItems.Add(null);

        equipmentItems[equipmentIndex] = item;
    }

    private bool CanPlaceItemInSlot(InventoryItem item, InventorySlotUI slot)
    {
        if (slot == null)
            return false;

        if (item == null)
            return true;

        if (!slot.IsEquipmentSlot())
            return true;

        return item.itemType == slot.GetAcceptedItemType();
    }
    public void RemoveEquipmentItemAtSlot(int slotIndex)
{
    if (slotIndex < 0 || slotIndex >= equipmentItems.Count)
        return;

    equipmentItems[slotIndex] = null;
    RefreshUI();
}

    public bool MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || toIndex < 0)
            return false;

        InventorySlotUI fromSlot = GetSlotByIndex(fromIndex);
        InventorySlotUI toSlot = GetSlotByIndex(toIndex);

        if (fromSlot == null || toSlot == null)
            return false;

        InventoryItem fromItem = GetItemByIndex(fromIndex);
        InventoryItem toItem = GetItemByIndex(toIndex);

        if (fromItem == null)
            return false;

        if (!CanPlaceItemInSlot(fromItem, toSlot))
            return false;

        if (!CanPlaceItemInSlot(toItem, fromSlot))
            return false;

        SetItemByIndex(toIndex, fromItem);
        SetItemByIndex(fromIndex, toItem);

        RefreshUI();
        return true;
    }

    public bool DropItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0)
            return false;

        InventoryItem item = GetItemByIndex(slotIndex);
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

        Collider col = droppedObject.GetComponentInChildren<Collider>();
        if (col != null)
            col.isTrigger = true;

        Rigidbody rb = droppedObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = droppedObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        WorldItemPickup pickup = droppedObject.GetComponent<WorldItemPickup>();
        if (pickup == null)
            pickup = droppedObject.AddComponent<WorldItemPickup>();

        pickup.SetItem(item);

        SetItemByIndex(slotIndex, null);
        RefreshUI();
        return true;
    }

    public void RefreshUI()
    {
        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].ClearSlot();

                if (i < items.Count && items[i] != null)
                    slots[i].SetItem(items[i], itemIconPrefab);
            }
        }

        if (equipmentSlots != null)
        {
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                equipmentSlots[i].ClearSlot();

                if (i < equipmentItems.Count && equipmentItems[i] != null)
                    equipmentSlots[i].SetItem(equipmentItems[i], itemIconPrefab);
            }
        }
    }
}