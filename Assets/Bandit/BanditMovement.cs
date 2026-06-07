using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class BanditMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.0f;
    public float rotationSpeed = 10f;
    public float gravity = 9.81f;

    private CharacterController controller;
    private Animator animator;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Vector2 input = Vector2.zero;
        bool isRunning = false;

#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            if (keyboard.leftShiftKey.isPressed) isRunning = true;
        }
#else
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(KeyCode.LeftShift)) isRunning = true;
#endif

        input = input.normalized;

        // Camera relative movement
        Vector3 moveDirection = Vector3.zero;
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            moveDirection = camForward * input.y + camRight * input.x;
        }
        else
        {
            moveDirection = new Vector3(input.x, 0, input.y);
        }

        float speed = input.magnitude > 0 ? (isRunning ? runSpeed : walkSpeed) : 0f;

        // Calculate velocity
        Vector3 velocity = moveDirection * speed;

        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f; // Small constant downward force to stay grounded
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        velocity.y = verticalVelocity;

        // Move the controller
        controller.Move(velocity * Time.deltaTime);

        // Rotation
        if (moveDirection.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Animator parameters
        if (animator != null)
        {
            // Speed thresholds: 0 = Idle, 1.5 = Walk, 4.0 = Run
            animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        }
    }
}
