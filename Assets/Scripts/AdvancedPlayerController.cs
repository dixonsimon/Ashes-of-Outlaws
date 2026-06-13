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
    private Transform rootBone;
    private Vector3 hipsOriginalPos;
    private Quaternion hipsOriginalRot;
    private Vector3 rootOriginalPos;
    private Quaternion rootOriginalRot;
    private float currentSpeed;
    private Vector3 currentMoveDirection;
    private float pelvisOriginalPlayerLocalX;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        
        if (animator != null)
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (hipsBone != null)
            {
                hipsOriginalPos = hipsBone.localPosition;
                hipsOriginalRot = hipsBone.localRotation;
                pelvisOriginalPlayerLocalX = transform.InverseTransformPoint(hipsBone.position).x;
                rootBone = hipsBone.parent;
                if (rootBone != null)
                {
                    rootOriginalPos = rootBone.localPosition;
                    rootOriginalRot = rootBone.localRotation;
                }
            }
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
        Vector3 targetMoveDirection = Vector3.zero;
        if (mainCamera != null && input.magnitude > 0.01f)
        {
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            targetMoveDirection = camForward * input.y + camRight * input.x;
        }
        else if (input.magnitude > 0.01f)
        {
            targetMoveDirection = new Vector3(input.x, 0, input.y);
        }

        if (input.magnitude > 0.01f)
        {
            // Smoothly slerp direction when changing inputs
            if (currentMoveDirection.magnitude < 0.01f)
            {
                currentMoveDirection = targetMoveDirection;
            }
            else
            {
                currentMoveDirection = Vector3.Slerp(currentMoveDirection, targetMoveDirection, 12f * Time.deltaTime).normalized;
            }
        }

        float targetSpeed = input.magnitude > 0.01f ? (isRunning ? runSpeed : walkSpeed) : 0f;
        
        // Smoothly accelerate and decelerate to synchronize physical speed with animator state
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, (targetSpeed > currentSpeed ? 12f : 16f) * Time.deltaTime);

        // Calculate velocity (allows smooth deceleration along the last heading when keys are released)
        Vector3 velocity = currentMoveDirection * currentSpeed;

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

        // Rotation - rotate towards the movement direction smoothly when active
        if (currentMoveDirection.magnitude > 0.01f && currentSpeed > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Reset movement vector completely when stopped
        if (currentSpeed < 0.01f)
        {
            currentMoveDirection = Vector3.zero;
        }

        // Animator parameters
        if (animator != null)
        {
            // Speed thresholds: 0 = Idle, 2 = Walk, 5.33 = Run
            animator.SetFloat("Speed", currentSpeed);
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
        // Stabilize intermediate root bone to prevent root-level translation/rotation sway.
        if (rootBone != null && rootBone.name == "root")
        {
            rootBone.localPosition = Vector3.zero;
            rootBone.localRotation = Quaternion.identity;
        }

        // Stabilize hips (pelvis) bone to prevent lateral drift/weaving while running
        if (hipsBone != null)
        {
            // Convert pelvis position to player local space
            Vector3 playerLocalPos = transform.InverseTransformPoint(hipsBone.position);

            // Damp the X-coordinate (lateral sway) by 75% towards its original offset (which is 0)
            float targetX = Mathf.Lerp(pelvisOriginalPlayerLocalX, playerLocalPos.x, 0.25f);
            playerLocalPos.x = Mathf.Lerp(playerLocalPos.x, targetX, 10f * Time.deltaTime);

            // Convert back to world space and apply
            hipsBone.position = transform.TransformPoint(playerLocalPos);

            // Damp hips local rotation towards its original bind-pose rotation by 60%
            Quaternion targetRot = Quaternion.Slerp(hipsBone.localRotation, hipsOriginalRot, 0.4f); // 0.4f means keeping 40% of rotation (damped by 60%)
            hipsBone.localRotation = Quaternion.Slerp(hipsBone.localRotation, targetRot, 8f * Time.deltaTime);
        }
    }
}
