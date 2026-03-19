using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject itemIconPrefab;

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

        items.Add(newItem);
        RefreshUI();
    }

    public List<InventoryItem> GetItems()
    {
        return items;
    }

    public void RefreshUI()
    {
        if (slots == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearSlot();

            if (i < items.Count)
            {
                slots[i].SetItem(items[i], itemIconPrefab);
            }
        }
    }
}