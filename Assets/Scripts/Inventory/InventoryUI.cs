using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    private InputAction toggleInventoryAction;
    private bool isOpen;

    private void Awake()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        var inputActions = InputSystem.actions;
        toggleInventoryAction = inputActions.FindAction("UI/ToggleInventory");
    }

    private void OnEnable()
    {
        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.Enable();
            toggleInventoryAction.performed += OnToggleInventory;
        }
    }

    private void OnDisable()
    {
        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.performed -= OnToggleInventory;
            toggleInventoryAction.Disable();
        }
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        isOpen = !isOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isOpen);

        Time.timeScale = isOpen ? 0f : 1f;

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Debug.Log("Inventory toggled. Open = " + isOpen);
    }
}