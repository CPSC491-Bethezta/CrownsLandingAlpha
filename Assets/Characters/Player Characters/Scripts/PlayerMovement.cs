using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float rotationSpeed = 720f;         // deg/sec
    public float gravity = -20f;
    public float groundedGravity = -2f;

    [Header("Camera")]
    // Assign Main Camera or a CameraTarget (recommended) in the Inspector
    public Transform cameraRoot;

    private CharacterController controller;
    private Vector2 moveInput;
    private float verticalVel;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!cameraRoot && Camera.main) cameraRoot = Camera.main.transform;
    }

    // Hook this via PlayerInput "On Move"
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        // --- Camera-relative planed basis ---
        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;
        if (cameraRoot)
        {
            camForward = Vector3.ProjectOnPlane(cameraRoot.forward, Vector3.up).normalized;
            camRight = Vector3.ProjectOnPlane(cameraRoot.right, Vector3.up).normalized;
        }

        // Input to world-space
        Vector3 inputDir = (camForward * moveInput.y + camRight * moveInput.x);
        inputDir = inputDir.sqrMagnitude > 1f ? inputDir.normalized : inputDir;

        // Horizontal velocity
        Vector3 horizVel = inputDir * walkSpeed;

        // Face move direction
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
    }
}
