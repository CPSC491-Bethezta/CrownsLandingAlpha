using UnityEngine;

public enum InventoryItemType
{
    General,
    Weapon,
    Potion,
    Armor,
    Utility
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject itemObject;
    public GameObject equippedObject;
    public InventoryItemType itemType = InventoryItemType.General;
}