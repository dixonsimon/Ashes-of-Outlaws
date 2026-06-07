using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class AdvancedPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.33f;
    public float rotationSpeed = 10.0f;
    public float gravity = 15.0f;

    private CharacterController controller;
    private Animator animator;
    private float verticalVelocity;
    private Camera mainCamera;
    private Transform hipsBone;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        
        if (animator != null)
        {
            hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
        }
        
        // Setup Foliage Touch Bending dynamically
        SetupFoliageInteraction();
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

        // Calculate velocity (directly along camera-relative direction)
        Vector3 velocity = moveDirection * speed;

        if (controller.isGrounded)
        {
            verticalVelocity = -2.5f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        velocity.y = verticalVelocity;

        // Move the controller
        controller.Move(velocity * Time.deltaTime);

        // Rotation - rotate towards the movement direction smoothly
        if (moveDirection.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Animator parameters
        if (animator != null)
        {
            // Speed thresholds: 0 = Idle, 2 = Walk, 5.33 = Run
            animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        }
    }

    private void SetupFoliageInteraction()
    {
        // Dynamic reflection to add NatureCollider if Nature Renderer package is present
        System.Type colliderType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            colliderType = assembly.GetType("VisualDesignCafe.Rendering.Nature.NatureCollider");
            if (colliderType != null) break;
        }

        if (colliderType != null)
        {
            if (GetComponent(colliderType) == null)
            {
                gameObject.AddComponent(colliderType);
                Debug.Log("Foliage bending collider successfully attached to player!");
            }
        }
        else
        {
            Debug.Log("Nature Renderer package not detected in assemblies. Foliage touch-bending collider will be attached dynamically once Nature Renderer is imported.");
        }
    }

    void LateUpdate()
    {
        // Stabilize hips side-to-side locomotion sway to keep path visually straight
        if (hipsBone != null && animator != null)
        {
            float speed = animator.GetFloat("Speed");
            if (speed > 0.05f)
            {
                Vector3 localPos = hipsBone.localPosition;
                // Smoothly blend local X to 0 based on speed to avoid sudden snaps
                localPos.x = Mathf.Lerp(localPos.x, 0f, 10f * Time.deltaTime);
                hipsBone.localPosition = localPos;
            }
        }
    }
}
