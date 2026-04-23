using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central controller for the in-game menu. Owns the InGameMenu toggle (Tab),
/// exclusive panel switching, cursor/time-scale state, and HUD visibility (minimap).
/// </summary>
public class GameMenuController : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private GameObject inGameMenu;

    [Header("Panels")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private GameObject mapPanel;

    [Header("HUD")]
    [SerializeField] private GameObject minimap;

    private InputAction toggleMenuAction;
    private bool isOpen;

    private void Awake()
    {
        SetMenuOpen(false);

        var inputActions = InputSystem.actions;
        toggleMenuAction = inputActions.FindAction("UI/ToggleInventory");
    }

    private void OnEnable()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.Enable();
            toggleMenuAction.performed += OnToggleMenu;
        }
    }

    private void OnDisable()
    {
        if (toggleMenuAction != null)
        {
            toggleMenuAction.performed -= OnToggleMenu;
            toggleMenuAction.Disable();
        }
    }

    private void OnToggleMenu(InputAction.CallbackContext context) => SetMenuOpen(!isOpen);

    /// <summary>Called by the Inv nav button.</summary>
    public void ShowInventory() => ShowPanel(inventoryPanel);

    /// <summary>Called by the Quests nav button.</summary>
    public void ShowQuests() => ShowPanel(questPanel);

    /// <summary>Called by the Map nav button.</summary>
    public void ShowMap() => ShowPanel(mapPanel);

    // ── internals ─────────────────────────────────────────────────────────────

    private void SetMenuOpen(bool open)
    {
        isOpen = open;

        if (inGameMenu != null) inGameMenu.SetActive(open);
        if (minimap != null)    minimap.SetActive(!open);

        if (open)
            ShowPanel(inventoryPanel);
        else
            HideAllPanels();

        Time.timeScale = open ? 0f : 1f;
        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void ShowPanel(GameObject target)
    {
        HideAllPanels();
        if (target != null)
            target.SetActive(true);
    }

    private void HideAllPanels()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (questPanel != null)     questPanel.SetActive(false);
        if (mapPanel != null)       mapPanel.SetActive(false);
    }
}
