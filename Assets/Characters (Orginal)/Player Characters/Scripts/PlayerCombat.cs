using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Name of the trigger parameter that plays the punch animation.")]
    public string punchTriggerName = "Attack";

    private Animator animator;
    private bool punchQueued;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPunch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !punchQueued)
        {
            punchQueued = true;
            animator.SetTrigger(punchTriggerName);
        }

        if (ctx.canceled)
            punchQueued = false;
    }
}
