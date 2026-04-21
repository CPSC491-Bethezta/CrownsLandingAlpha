using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class WorldItemPickup : MonoBehaviour
{
    [SerializeField] private InventoryItem itemData;

    private bool playerInRange = false;

    public void SetItem(InventoryItem item)
    {
        itemData = item;
    }

    private bool IsAttachedToPlayer()
    {
        return transform.root.CompareTag("Player");
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (IsAttachedToPlayer())
            return;

        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (itemData != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemData);

                if (PickupPromptUI.Instance != null)
                    PickupPromptUI.Instance.Hide();

                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsAttachedToPlayer())
            return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (PickupPromptUI.Instance != null)
                PickupPromptUI.Instance.Show();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsAttachedToPlayer())
            return;

        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (PickupPromptUI.Instance != null)
                PickupPromptUI.Instance.Hide();
        }
    }
}