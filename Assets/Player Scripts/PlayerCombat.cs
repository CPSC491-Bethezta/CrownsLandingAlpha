using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerCombat
/// ------------------------------
/// Responsibility:
/// - Toggles a "sword stance" state (on/off).
/// - Allows punching only while in stance.
/// - Shows/hides the sword model based on stance.
/// - Optionally locks movement briefly during a punch.
///
/// Dependencies & Assumptions:
/// - Requires an <see cref="Animator"/> on the same GameObject.
/// - Animator must define:
///   Trigger: punchTriggerName (e.g., "Punch")
///   Trigger: swordStanceTriggerName (e.g., "SwordStance")
///   Bool: "InStance" (used to drive stance locomotion layer/blend tree)
/// - Input System (New): UI or Player Input must call OnPunch / OnStance.
/// - Optional: <see cref="PlayerMovement"/> may be present to lock movement during attacks.
///
/// Typical Usage:
/// - Attach to the Player root with Animator + (optionally) PlayerMovement.
/// - Assign a sword GameObject (child in hand) to <see cref="swordModel"/> in the Inspector.
/// - Bind input actions so that OnPunch and OnStance get invoked.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Animator trigger for punch animation. Must exist as a Trigger parameter.")]
    public string punchTriggerName = "Punch";

    [Tooltip("Animator trigger for sword stance animation. Must exist as a Trigger parameter.")]
    public string swordStanceTriggerName = "SwordStance";

    [Header("Sword Model")]
    [Tooltip("Reference to the sword GameObject in the player's hand. Toggled on/off with stance.")]
    public GameObject swordModel; // Assign in Inspector (optional, but recommended)

    [Header("Settings")]
    [Tooltip("How long to lock player movement when punching (seconds). Requires PlayerMovement component.")]
    public float movementLockTime = 0.2f;

    // Cached references (set in Awake)
    private Animator animator;            // Required; drives punch and stance animations
    private PlayerMovement movement;      // Optional; used to momentarily lock movement during punch

    // Internal state flags
    private bool punchQueued;             // Prevents multiple punch triggers in a single press
    private bool inStance;                // True when sword stance is active

    /// <summary>
    /// Cache components and set initial visual state.
    /// Ensures the sword is hidden at startup (non-stance).
    /// </summary>
    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

        // Defensive: Sword should start hidden unless design requires otherwise.
        if (swordModel)
            swordModel.SetActive(false);
    }

    // -------- Input System Callbacks --------
    // These are intended to be wired via Unity's Input System (PlayerInput or C# event hookup).
    // They expect an action bound to "Punch" and one bound to "Stance" in your Input Actions asset.

    /// <summary>
    /// Input callback for punch.
    /// Only triggers punch if currently in stance.
    /// Locks movement briefly (if PlayerMovement exists) to avoid foot sliding and
    /// to ensure hit timing feels crisp.
    /// </summary>
    /// <param name="ctx">Input action callback context (performed/canceled)</param>
    public void OnPunch(InputAction.CallbackContext ctx)
    {
        // Only act on 'performed' to avoid multiple triggers (e.g., press + hold).
        if (ctx.performed && !punchQueued)
        {
            // Design rule: Punching is allowed only while in sword stance.
            if (inStance)
            {
                punchQueued = true;

                // Fire the punch animation. Ensure the Animator has a matching Trigger.
                animator.SetTrigger(punchTriggerName);

                // Optional: briefly lock locomotion for animation fidelity.
                // This reduces sliding/misaligned footwork during the strike.
                if (movement)
                    movement.RequestMovementLock(movementLockTime);
            }
        }

        // Reset the queue on release to allow a new punch press.
        if (ctx.canceled)
            punchQueued = false;
    }

    /// <summary>
    /// Input callback for toggling stance (on/off).
    /// Updates Animator bool "InStance", plays stance transition trigger,
    /// and toggles the sword model visibility to match the state.
    /// </summary>
    /// <param name="ctx">Input action callback context (performed)</param>
    public void OnStance(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            // Flip stance state
            inStance = !inStance;

            // Animator bool drives stance layers/blends (e.g., upper-body mask, locomotion set).
            animator.SetBool("InStance", inStance);

            // Trigger a stance enter/exit transition (optional but recommended for smoother blends).
            animator.SetTrigger(swordStanceTriggerName);

            // Visual authority: show sword only when we are in stance.
            if (swordModel)
                swordModel.SetActive(inStance);
        }
    }
}
