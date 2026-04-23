using System.Collections.Generic;
using UnityEngine;

public class EnemyLootDropper : MonoBehaviour
{
    [System.Serializable]
    public class LootEntry
    {
        public InventoryItem item;
        [Range(0f, 1f)] public float dropChance = 0.5f;
    }

    [Header("Drop Settings")]
    [SerializeField] private GameObject worldPickupPrefab;

    [Header("Loot Table")]
    [SerializeField] private List<LootEntry> lootTable = new List<LootEntry>();

    private bool hasDropped = false;

    public void DropLoot()
    {
        if (hasDropped) return;
        hasDropped = true;

        foreach (var entry in lootTable)
        {
            if (entry.item == null) continue;

            if (Random.value <= entry.dropChance)
            {
                SpawnPickup(entry.item);
                return; // drop only ONE item for now
            }
        }
    }

    private void SpawnPickup(InventoryItem item)
    {
        if (worldPickupPrefab == null)
        {
            Debug.LogWarning("No pickup prefab assigned!");
            return;
        }

        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

        GameObject obj = Instantiate(worldPickupPrefab, spawnPos, Quaternion.identity);

        WorldItemPickup pickup = obj.GetComponent<WorldItemPickup>();

        if (pickup != null)
        {
            pickup.SetItem(item);
        }
    }
}