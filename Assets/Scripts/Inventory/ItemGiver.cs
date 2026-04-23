using UnityEngine;

/// <summary>
/// Attach to an NPC. Holds a reference to an item that can be given
/// to the player via dialogue or other interactions.
/// Follows the same pattern as <see cref="QuestGiver"/>.
/// </summary>
public class ItemGiver : MonoBehaviour
{
    [SerializeField] private InventoryItem item;

    /// <summary>Adds the held item to the player's inventory. Safe to call multiple times — only gives once.</summary>
    public void GiveItem()
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemGiver] No item assigned.", this);
            return;
        }

        InventoryManager.Instance?.AddItem(item);
        Debug.Log($"[ItemGiver] Gave player: {item.itemName}");

        // Prevent giving the same item again on repeat visits
        item = null;
    }
}
