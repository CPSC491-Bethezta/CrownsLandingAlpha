using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
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

    [Tooltip("Animator trigger for Whirlwind melee animation. Must exist as a Trigger parameter.")]
    public string AoeMeleeTriggerName = "AOE";

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

    // Combat variables
    public float range = 500f;
    public float damage = 10f;
    public LayerMask hitMask = ~0;
    bool isEquipped;

    private float hitDelay = 0.4f; 
    private float lastAttackTime = -999f;
    private bool  isAttacking;
[SerializeField] private float attackCooldown = 0.5f;

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
            if (inStance)
            {
            //check if currently attacking
            if (isAttacking)
                return;

            // CD check
            if (Time.time < lastAttackTime + attackCooldown)
                return;

            // Start a new attack
            punchQueued   = true;
            isAttacking   = true;
            lastAttackTime = Time.time;
                punchQueued = true;
                animator.SetTrigger(punchTriggerName);
                StartCoroutine(DelayedHit());
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
        private IEnumerator DelayedHit()
{
    yield return new WaitForSeconds(hitDelay);

    Fire();

    isAttacking = false;
}
    void Fire()
{
    // start ray at chest height instead of camera from the old script
    Vector3 origin = transform.position + Vector3.up * 1.0f;
    Vector3 direction = transform.forward;

    if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
    {
        var m = hit.collider.GetComponentInParent<Enemy>();
        if (m != null)
        {
            m.TakeDamage(damage);
        }
    }

    // optional: visualize in Scene view - chat gpt
Debug.DrawRay(origin, direction * range, Color.red, 0.1f);
}


// --- AOE Melee stuff
[SerializeField] private float aoeRadius = 3f;
[SerializeField] private int aoeDamage = 30;
[SerializeField] private LayerMask enemyLayer;

[SerializeField] private float aoeCooldown = 1.25f;
[SerializeField] private float aoeHitDelay = 0.15f;  // when damage should apply (sync to animation)

//prevents holding button for damage
private bool aoeQueued = false;
private float lastAoeTime = -999f;
public void OnAOE(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
{
    // Only act on 'performed' to avoid multiple triggers (press + hold)
    if (ctx.performed && !aoeQueued)
    {
        if (!inStance) return;

        // check if currently attacking
        if (isAttacking) return;

        // CD check
        if (Time.time < lastAoeTime + aoeCooldown) return;

        // Start AOE attack
        aoeQueued   = true;
        isAttacking = true;
        lastAoeTime = Time.time;

        // Play "jump" animation (ground slam)
        if (animator && !string.IsNullOrEmpty(AoeMeleeTriggerName))
            animator.SetTrigger(AoeMeleeTriggerName);

        StartCoroutine(AoeDelayedHit());

        if (movement)
            movement.RequestMovementLock(movementLockTime);
    }

    // Reset the queue on release to allow a new press.
    if (ctx.canceled)
        aoeQueued = false;
}

private System.Collections.IEnumerator AoeDelayedHit()
{
    yield return new WaitForSeconds(aoeHitDelay);

    Vector3 center = transform.position + Vector3.up * 1.0f;

    Collider[] hits = Physics.OverlapSphere(center, aoeRadius, hitMask, QueryTriggerInteraction.Ignore);

    for (int i = 0; i < hits.Length; i++)
    {
        var enemy = hits[i].GetComponentInParent<Enemy>();
        if (enemy != null)
            enemy.TakeDamage(damage);
    }


    isAttacking = false;
}

}
