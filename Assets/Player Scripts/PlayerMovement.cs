using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerMovement
/// ------------------------------
/// Responsibility:
/// - Reads WASD/LeftStick and Sprint inputs (Unity Input System).
/// - Moves a CharacterController in a camera-relative plane.
/// - Rotates the character toward the move direction.
/// - Applies gravity with a slightly sticky grounded gravity to avoid micro-bounces.
/// - Exposes a movement lock API for systems like combat to freeze locomotion
///   while preserving the current locomotion pose in the Animator.
///
/// Dependencies & Animator:
/// - Requires <see cref="CharacterController"/> (enforced via RequireComponent).
/// - Optional <see cref="Animator"/> with a float parameter "Speed" (0..1).
///   "Speed" drives locomotion blend trees; it’s damped for smooth transitions.
/// - Optional camera root reference; if null, falls back to Camera.main.
///
/// Input System:
/// - Bind "Move" (Vector2) -> OnMove
/// - Bind "Sprint" (Button) -> OnSprint
///
/// Extension Points:
/// - Call <see cref="RequestMovementLock(float)"/> from combat/interaction to briefly freeze motion.
/// - Replace "Speed" parameter name or plug in additional Animator params as needed.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    /// <summary>Base walking speed in m/s.</summary>
    public float walkSpeed = 5f;            // base move speed

    /// <summary>Speed multiplier applied while sprinting.</summary>
    public float sprintMultiplier = 2f;     // hold Shift to multiply speed

    /// <summary>Degrees per second to rotate toward input direction.</summary>
    public float rotationSpeed = 720f;      // deg/sec turn toward move dir

    /// <summary>Downward acceleration (negative). Higher magnitude = snappier fall.</summary>
    public float gravity = -20f;            // stronger than -9.81 for snappy feel

    /// <summary>
    /// Small constant downward velocity when grounded to keep the controller anchored,
    /// preventing tiny airborne frames on uneven ground.
    /// </summary>
    public float groundedGravity = -2f;     // small push to stay grounded

    [Header("Camera")]
    /// <summary>
    /// Camera root used to compute camera-relative movement (usually a pivot on the player).
    /// If not set, falls back to <see cref="Camera.main"/>.
    /// </summary>
    public Transform cameraRoot;

    // --- Components ---
    private CharacterController controller; // Required movement driver
    private Animator animator;              // Optional: feeds "Speed" for blend trees

    // --- Input State ---
    private Vector2 moveInput;              // (-1..1, -1..1) from Input System
    private bool isSprinting;               // True while sprint is held

    // --- Gravity ---
    private float verticalVel;              // y-velocity integrated each frame

    // --- External Movement Lock ---
    // When > 0, both translation and rotation are paused.
    private float externalLockTimer;
    /// <summary>True while an external system (e.g., combat) has locked movement.</summary>
    public bool IsMovementLocked => externalLockTimer > 0f;

    // During a lock, we freeze the Animator "Speed" at the captured value
    // so the pose stays consistent (e.g., mid-run) rather than dropping to Idle.
    private float animatorSpeedHold = 0f;               // 0..1
    private bool freezeAnimatorSpeedDuringLock = false; // True while locked

    /// <summary>
    /// Cache required components and set camera fallback.
    /// </summary>
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Sensible fallback: if no explicit camera root is assigned, use the main camera.
        if (!cameraRoot && Camera.main) cameraRoot = Camera.main.transform;
    }

    /// <summary>
    /// Public API for other systems to freeze movement/rotation for a fixed duration.
    /// Captures the current normalized planar speed to maintain locomotion pose.
    /// </summary>
    /// <param name="seconds">Lock duration in seconds (non-cumulative; takes the max).</param>
    public void RequestMovementLock(float seconds)
    {
        // Capture current animator speed once at (re)start of a lock.
        animatorSpeedHold = GetCurrentNormalizedPlanarSpeed();

        // If overlapping locks occur, keep the longer remaining duration.
        externalLockTimer = Mathf.Max(externalLockTimer, seconds);

        // While locked, Animator "Speed" will be held.
        freezeAnimatorSpeedDuringLock = true;
    }

    /// <summary>
    /// Input System callback: reads 2D move vector (WASD/Stick).
    /// </summary>
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// Input System callback: toggles sprint while performed; clears on canceled.
    /// </summary>
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) isSprinting = true;
        else if (ctx.canceled) isSprinting = false;
    }

    /// <summary>
    /// Per-frame locomotion update:
    /// - Ticks lock timer
    /// - Computes camera-relative direction
    /// - Applies rotation, gravity, and CharacterController.Move
    /// - Drives Animator "Speed" with smoothing (or held value during lock)
    /// </summary>
    void Update()
    {
        float dt = Time.deltaTime;

        // 1) Tick down any external movement lock.
        if (externalLockTimer > 0f)
        {
            externalLockTimer -= dt;
            if (externalLockTimer <= 0f)
            {
                // Unlock: resume live "Speed" updates.
                freezeAnimatorSpeedDuringLock = false;
            }
        }

        // 2) Build camera-relative basis on the XZ plane.
        //    We ignore camera pitch to keep planar movement intuitive.
        Vector3 camF = Vector3.forward;
        Vector3 camR = Vector3.right;
        if (cameraRoot)
        {
            camF = Vector3.ProjectOnPlane(cameraRoot.forward, Vector3.up).normalized;
            camR = Vector3.ProjectOnPlane(cameraRoot.right, Vector3.up).normalized;
        }

        // 3) Convert input to a world-space direction (flattened).
        //    If locked, zero translation (but we preserve animator pose separately).
        Vector3 inputDir = IsMovementLocked
            ? Vector3.zero
            : (camF * moveInput.y + camR * moveInput.x);

        // Clamp diagonal magnitude to 1 so speed is uniform in all directions.
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        // 4) Compute horizontal velocity with sprint modifier.
        float currentSpeed = walkSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 horizVel = inputDir * currentSpeed;

        // 5) Rotate toward movement direction (skip while locked or when nearly zero input).
        if (!IsMovementLocked && inputDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotationSpeed * dt);
        }

        // 6) Gravity integration. Use a small constant while grounded to stay snapped.
        bool grounded = controller.isGrounded;
        if (grounded && verticalVel < 0f)
            verticalVel = groundedGravity;          // sticky ground
        else
            verticalVel += gravity * dt;            // normal gravity when airborne

        // 7) Final move. Translation is already zeroed if locked.
        Vector3 velocity = horizVel + Vector3.up * verticalVel;
        controller.Move(velocity * dt);

        // 8) Animator drive: use held "Speed" while locked; otherwise live planar speed.
        float normalized = freezeAnimatorSpeedDuringLock
            ? animatorSpeedHold
            : GetCurrentNormalizedPlanarSpeed();

        if (animator)
        {
            // Damping (0.1s) smooths small spikes to avoid footstep pops.
            animator.SetFloat("Speed", normalized, 0.1f, dt);
        }
    }

    /// <summary>
    /// Computes the current planar (XZ) speed normalized to 0..1 where:
    /// 0 = idle and 1 = full sprint (walkSpeed * sprintMultiplier).
    /// This also works when InStance is activated.
    /// </summary>
    private float GetCurrentNormalizedPlanarSpeed()
    {
        float planarSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
        return Mathf.Clamp(planarSpeed / (walkSpeed * sprintMultiplier), 0f, 1f);
    }
}
