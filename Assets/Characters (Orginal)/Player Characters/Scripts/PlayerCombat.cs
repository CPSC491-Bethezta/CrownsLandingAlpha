using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles stance toggling and punching. 
/// Also shows/hides the sword model when stance is active.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Animator trigger for punch animation.")]
    public string punchTriggerName = "Punch";

    [Tooltip("Animator trigger for sword stance animation.")]
    public string swordStanceTriggerName = "SwordStance";

    [Header("Sword Model")]
    [Tooltip("Reference to the sword GameObject in the player's hand.")]
    public GameObject swordModel; // assign in Inspector

    [Header("Settings")]
    [Tooltip("How long to lock player movement when punching.")]
    public float movementLockTime = 0.2f;

    private Animator animator;
    private PlayerMovement movement;

    private bool punchQueued;
    private bool inStance; // true when sword stance is active

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

        // Ensure sword starts hidden
        if (swordModel)
            swordModel.SetActive(false);
    }

    // --- Input System Callbacks ---

    public void OnPunch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !punchQueued)
        {
            if (inStance) // Only punch if in stance
            {
                punchQueued = true;
                animator.SetTrigger(punchTriggerName);

                if (movement)
                    movement.RequestMovementLock(movementLockTime);
            }
        }

        if (ctx.canceled)
            punchQueued = false;
    }

    public void OnStance(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inStance = !inStance;
            animator.SetBool("InStance", inStance);
            animator.SetTrigger(swordStanceTriggerName);

            // Toggle sword visibility
            if (swordModel)
                swordModel.SetActive(inStance);
        }
    }

  
}
