using UnityEngine;
using UnityEngine.InputSystem;

public class ChestLoot : MonoBehaviour
{
    [SerializeField] private InventoryItem[] possibleLoot;
    [SerializeField] private string openTriggerName = "Open";

    private bool playerInRange = false;
    private bool hasOpened = false;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!playerInRange || hasOpened)
            return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (hasOpened)
            return;

        hasOpened = true;

        if (animator != null)
            animator.SetTrigger(openTriggerName);

        GiveRandomLoot();
    }

    private void GiveRandomLoot()
    {
        if (possibleLoot == null || possibleLoot.Length == 0)
            return;

        InventoryItem randomItem = possibleLoot[Random.Range(0, possibleLoot.Length)];

        if (randomItem == null)
            return;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.AddItem(randomItem);

        if (LootPopupUI.Instance != null)
            LootPopupUI.Instance.ShowLoot(randomItem.itemName);

        if (PickupPromptUI.Instance != null)
            PickupPromptUI.Instance.Hide();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (PickupPromptUI.Instance != null)
                PickupPromptUI.Instance.Show();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (PickupPromptUI.Instance != null)
                PickupPromptUI.Instance.Hide();
        }
    }
}
