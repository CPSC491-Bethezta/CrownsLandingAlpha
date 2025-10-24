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

    private CharacterController controller;
    private Animator animator;
    private Vector2 moveInput;
    private float verticalVel;
    private bool isSprinting;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (!cameraRoot && Camera.main) cameraRoot = Camera.main.transform;
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
        // --- Camera-relative planar basis ---
        Vector3 camF = Vector3.forward;
        Vector3 camR = Vector3.right;
        if (cameraRoot)
        {
            camF = Vector3.ProjectOnPlane(cameraRoot.forward, Vector3.up).normalized;
            camR = Vector3.ProjectOnPlane(cameraRoot.right,   Vector3.up).normalized;
        }

        // Input → world direction (flattened)
        Vector3 inputDir = (camF * moveInput.y + camR * moveInput.x);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        // Speed (walk or sprint)
        float currentSpeed = walkSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 horizVel = inputDir * currentSpeed;

        // Face movement direction
        if (inputDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Gravity
        bool grounded = controller.isGrounded;
        if (grounded && verticalVel < 0f) verticalVel = groundedGravity;
        else verticalVel += gravity * Time.deltaTime;

        // Final move
        Vector3 velocity = horizVel + Vector3.up * verticalVel;
        controller.Move(velocity * Time.deltaTime);

        // --- Animation (Idle↔Walk blend) ---
        // Normalize to 0..1 because the Blend Tree currently has Idle@0, Walk@1 only.
        float planarSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
        float normalized = Mathf.Clamp(planarSpeed / (walkSpeed * sprintMultiplier), 0f, 1f);
        if (animator) animator.SetFloat("Speed", normalized, 0.1f, Time.deltaTime);
    }
}
