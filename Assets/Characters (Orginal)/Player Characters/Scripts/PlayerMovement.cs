using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;            // base move speed
    public float sprintMultiplier = 2f;     // hold Shift to multiply speed
    public float rotationSpeed = 720f;      // deg/sec turn toward move dir
    public float gravity = -20f;            // stronger than -9.81 for snappy feel
    public float groundedGravity = -2f;     // small push to stay grounded

    [Header("Camera")]
    // Assign a CameraTarget on your player (best) or the Main Camera transform
    public Transform cameraRoot;

    // --- Components ---
    private CharacterController controller;
    private Animator animator;

    // --- Input State ---
    private Vector2 moveInput;
    private bool isSprinting;

    // --- Gravity ---
    private float verticalVel;

    // --- External Movement Lock ---
    private float externalLockTimer;                    // while > 0, movement/rotation are disabled
    public bool IsMovementLocked => externalLockTimer > 0f;

    // NEW: hold the animator 'Speed' value captured at the moment the lock starts,
    // so locomotion pose doesn't drop to Idle during the lock.
    private float animatorSpeedHold = 0f;               // 0..1
    private bool freezeAnimatorSpeedDuringLock = false; // true while locked

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (!cameraRoot && Camera.main) cameraRoot = Camera.main.transform;
    }

    /// <summary>
    /// Allows other systems (e.g., combat) to freeze player movement for a duration.
    /// We capture the current locomotion Speed value so the Animator keeps that pose.
    /// </summary>
    public void RequestMovementLock(float seconds)
    {
        // Capture current animator speed once when lock (re)starts
        animatorSpeedHold = GetCurrentNormalizedPlanarSpeed();
        externalLockTimer = Mathf.Max(externalLockTimer, seconds);
        freezeAnimatorSpeedDuringLock = true;
    }

    // Input System callback (Player/Move)
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    // Input System callback (Player/Sprint mapped to <Keyboard>/leftShift)
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) isSprinting = true;
        else if (ctx.canceled) isSprinting = false;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Tick down movement lock
        if (externalLockTimer > 0f)
        {
            externalLockTimer -= dt;
            if (externalLockTimer <= 0f)
            {
                // unlock: resume live animator speed updates
                freezeAnimatorSpeedDuringLock = false;
            }
        }

        // --- Camera-relative planar basis ---
        Vector3 camF = Vector3.forward;
        Vector3 camR = Vector3.right;
        if (cameraRoot)
        {
            camF = Vector3.ProjectOnPlane(cameraRoot.forward, Vector3.up).normalized;
            camR = Vector3.ProjectOnPlane(cameraRoot.right, Vector3.up).normalized;
        }

        // Input → world direction (flattened). If locked, we stop translation.
        Vector3 inputDir = IsMovementLocked
            ? Vector3.zero
            : (camF * moveInput.y + camR * moveInput.x);

        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        // Speed (walk or sprint)
        float currentSpeed = walkSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 horizVel = inputDir * currentSpeed;

        // Face movement direction (only if not locked)
        if (!IsMovementLocked && inputDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotationSpeed * dt);
        }

        // Gravity
        bool grounded = controller.isGrounded;
        if (grounded && verticalVel < 0f) verticalVel = groundedGravity;
        else verticalVel += gravity * dt;

        // Final move (translation locks by zeroing horizVel via inputDir above)
        Vector3 velocity = horizVel + Vector3.up * verticalVel;
        controller.Move(velocity * dt);

        // --- Animator 'Speed' drive ---
        // If locked, keep feeding the held Speed value (pose stays in run/walk).
        // If not locked, use live planar speed from controller.
        float normalized = freezeAnimatorSpeedDuringLock
            ? animatorSpeedHold
            : GetCurrentNormalizedPlanarSpeed();

        if (animator)
            animator.SetFloat("Speed", normalized, 0.1f, dt);
    }

    /// <summary>
    /// Computes current locomotion Speed normalized to 0..1 (0=idle, 1=full sprint).
    /// </summary>
    private float GetCurrentNormalizedPlanarSpeed()
    {
        float planarSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
        return Mathf.Clamp(planarSpeed / (walkSpeed * sprintMultiplier), 0f, 1f);
    }
}
