using UnityEngine;

public class ItemDefinition : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
}

public enum ItemType
{
    Consumable,
    SpellFocus,

}
