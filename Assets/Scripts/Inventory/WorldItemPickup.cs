using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class WorldItemPickup : MonoBehaviour
{
    [SerializeField] private InventoryItem itemData;
    [SerializeField] private string interactTriggerName = "Interact";
    [SerializeField] private float pickupDelay = 0.15f;

    private bool playerInRange = false;
    private bool isPickingUp = false;
    private Animator playerAnimator;
    private PlayerAnimationController playerAnimationController;

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

        if (isPickingUp)
            return;

        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (itemData != null && InventoryManager.Instance != null)
            {
                StartCoroutine(PickupItem());
            }
        }
    }

    private IEnumerator PickupItem()
    {
        isPickingUp = true;

        if (playerAnimationController != null)
            playerAnimationController.InteractTrigger();
        else if (playerAnimator != null)
            playerAnimator.SetTrigger(interactTriggerName);

        yield return new WaitForSeconds(pickupDelay);

        InventoryManager.Instance.AddItem(itemData);

        if (PickupPromptUI.Instance != null)
            PickupPromptUI.Instance.Hide();

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsAttachedToPlayer())
            return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerAnimator = other.GetComponentInParent<Animator>();
            playerAnimationController = other.GetComponentInParent<PlayerAnimationController>();

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