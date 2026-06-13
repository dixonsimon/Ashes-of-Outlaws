using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BanditCameraOrbit : MonoBehaviour
{
    [Header("Target settings")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.3f, 0f); // Look at chest height

    [Header("Orbit settings")]
    public float distance = 4.5f;
    public float xSpeed = 150.0f;
    public float ySpeed = 150.0f;

    public float yMinLimit = -15f;
    public float yMaxLimit = 75f;

    public float smoothTime = 0.08f;

    private float x = 0.0f;
    private float y = 0.0f;

    private Vector3 currentRotation;
    private Vector3 rotationVelocity;

    [Header("Position Smoothing")]
    public float positionSmoothTime = 0.05f;
    private Vector3 cameraPositionVelocity;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Lock cursor by default for smooth rotation
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float mouseX = 0f;
        float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            var delta = mouse.delta.ReadValue();
            mouseX = delta.x * 0.05f;
            mouseY = delta.y * 0.05f;
        }
#else
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }
#endif

        x += mouseX * xSpeed * Time.deltaTime;
        y -= mouseY * ySpeed * Time.deltaTime;

        y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

        // Smooth rotation interpolation
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(y, x, 0), ref rotationVelocity, smoothTime);
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);

        // Target camera position relative to camera orientation (shoulder offset)
        // targetOffset.y is height, targetOffset.x is horizontal right offset
        Vector3 pivot = target.position + Vector3.up * targetOffset.y;
        Vector3 targetPosition = pivot - (rotation * Vector3.forward * distance) + (rotation * Vector3.right * targetOffset.x);

        // Smooth position interpolation to eliminate character controller terrain jitter
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref cameraPositionVelocity, positionSmoothTime);
        transform.rotation = rotation;

        // Escape to unlock cursor, Left Click to relock
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
#endif
    }
}
