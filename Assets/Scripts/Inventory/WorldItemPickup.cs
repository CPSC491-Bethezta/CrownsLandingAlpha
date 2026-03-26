using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class WorldItemPickup : MonoBehaviour
{
    [SerializeField] private InventoryItem itemData;
    [SerializeField] private GameObject interactText;

    private bool playerInRange = false;

    public void SetItem(InventoryItem item)
    {
        itemData = item;
    }

    private void Start()
    {
        if (interactText != null)
            interactText.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (itemData != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(itemData);

                if (interactText != null)
                    interactText.SetActive(false);

                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactText != null)
                interactText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactText != null)
                interactText.SetActive(false);
        }
    }
}