using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerNew : MonoBehaviour
{

    [SerializeField] private PlayerCombatController combatController;
    public static PlayerControllerNew  Instance
    {
        get
        {
            return s_Instance;
        }
    }

    private static PlayerControllerNew  s_Instance;

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

}
