using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerHub : MonoBehaviour
{

    [SerializeField] private PlayerCombatController combatController;
    [SerializeField] private QuestJournalUI questJournalUI;

    public static PlayerControllerHub  Instance
    {
        get
        {
            return s_Instance;
        }
    }

    private static PlayerControllerHub  s_Instance;

    private void Awake()
    {
        s_Instance = this;
        combatController = GetComponent<PlayerCombatController>();
    }

    public void OnCombatStance(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        Debug.Log("Called");
        if (combatController == null) return;
        combatController.OnStance(ctx);
    }

    public void OnJournal(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (questJournalUI == null) return;
        questJournalUI.Toggle();
    }

}
