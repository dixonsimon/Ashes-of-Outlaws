using UnityEngine;

public class BanditCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3.5f, -5.5f);
    public Vector3 lookAtOffset = new Vector3(0f, 1.2f, 0f);
    public float smoothSpeed = 8f;

    void LateUpdate()
    {
        if (target == null) return;

        // Target position based on player's position and offset
        Vector3 targetPosition = target.position + offset;

        // Smoothly interpolate camera position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Make the camera look at the player's chest/head height
        Vector3 lookPosition = target.position + lookAtOffset;
        transform.LookAt(lookPosition);
    }
}
