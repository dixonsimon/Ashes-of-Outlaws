using UnityEngine;

public class BanditFacialAnimator : MonoBehaviour
{
    [Header("Blink Settings")]
    public float blinkIntervalMin = 3.0f;
    public float blinkIntervalMax = 6.0f;
    public float blinkDuration = 0.15f;
    public float upperEyelidBlinkAngle = 22f; // Degrees of rotation around head's local right axis
    public float lowerEyelidBlinkAngle = -6f;  // Degrees of rotation around head's local right axis

    [Header("Eye Look Settings")]
    public float maxYawAngle = 10f;
    public float maxPitchAngle = 6f;
    public float saccadeIntervalMin = 1.5f;
    public float saccadeIntervalMax = 4.0f;
    public float eyeSpeed = 10f;

    [Header("Jaw & Brow Settings")]
    public float jawIdleMovement = 1.8f;      // Degrees of jaw opening
    public float jawFrequency = 1.2f;         // Speed of jaw breathing movement
    public float browResponseToEyes = 0.3f;    // Eyebrow movement scaling relative to vertical eye pitch

    // Cached bone transforms
    private Transform leftEye;
    private Transform rightEye;
    private Transform leftUpperEyelid;
    private Transform leftLowerEyelid;
    private Transform rightUpperEyelid;
    private Transform rightLowerEyelid;
    private Transform jaw;
    private Transform leftBrow;
    private Transform rightBrow;
    private Transform midBrow;

    // Original local rotations
    private Quaternion leftEyeOriginalRot;
    private Quaternion rightEyeOriginalRot;
    private Quaternion leftUpperEyelidOriginalRot;
    private Quaternion leftLowerEyelidOriginalRot;
    private Quaternion rightUpperEyelidOriginalRot;
    private Quaternion rightLowerEyelidOriginalRot;
    private Quaternion jawOriginalRot;
    private Quaternion leftBrowOriginalRot;
    private Quaternion rightBrowOriginalRot;
    private Quaternion midBrowOriginalRot;

    // State variables
    private bool isBlinking = false;
    private float blinkStartTime;
    private float nextBlinkTime;

    private float targetYaw = 0f;
    private float targetPitch = 0f;
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private float nextSaccadeTime;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("BanditFacialAnimator requires an Animator component on the same GameObject.");
            enabled = false;
            return;
        }

        Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
        if (head == null)
        {
            Debug.LogError("BanditFacialAnimator: Head bone not found in Animator Avatar.");
            enabled = false;
            return;
        }

        // Locate face bones recursively under head
        leftEye = FindDeepChild(head, "L_eye");
        rightEye = FindDeepChild(head, "R_eye");
        
        leftUpperEyelid = FindDeepChild(head, "L_upper_eyelid");
        leftLowerEyelid = FindDeepChild(head, "L_lower_eyelid");
        rightUpperEyelid = FindDeepChild(head, "R_upper_eyelid");
        rightLowerEyelid = FindDeepChild(head, "R_lower_eyelid");
        
        jaw = FindDeepChild(head, "C_jaw");
        
        leftBrow = FindDeepChild(head, "L_brow_mid");
        rightBrow = FindDeepChild(head, "R_brow_mid");
        midBrow = FindDeepChild(head, "C_brow_mid");

        // Save original rotations
        if (leftEye != null) leftEyeOriginalRot = leftEye.localRotation;
        if (rightEye != null) rightEyeOriginalRot = rightEye.localRotation;
        if (leftUpperEyelid != null) leftUpperEyelidOriginalRot = leftUpperEyelid.localRotation;
        if (leftLowerEyelid != null) leftLowerEyelidOriginalRot = leftLowerEyelid.localRotation;
        if (rightUpperEyelid != null) rightUpperEyelidOriginalRot = rightUpperEyelid.localRotation;
        if (rightLowerEyelid != null) rightLowerEyelidOriginalRot = rightLowerEyelid.localRotation;
        if (jaw != null) jawOriginalRot = jaw.localRotation;
        if (leftBrow != null) leftBrowOriginalRot = leftBrow.localRotation;
        if (rightBrow != null) rightBrowOriginalRot = rightBrow.localRotation;
        if (midBrow != null) midBrowOriginalRot = midBrow.localRotation;

        // Schedule first events
        ScheduleNextBlink();
        ScheduleNextSaccade();
    }

    void LateUpdate()
    {
        // 1. Process Blinking
        ProcessBlinking();

        // 2. Process Eye Movements (Saccades)
        ProcessEyeMovements();

        // 3. Process Breathing/Jaw Movements
        ProcessJawBreathing();

        // 4. Process Eyebrow Reactive Movement
        ProcessBrows();
    }

    private void ProcessBlinking()
    {
        if (isBlinking)
        {
            float progress = (Time.time - blinkStartTime) / blinkDuration;
            if (progress >= 1f)
            {
                isBlinking = false;
                ResetEyelids();
                ScheduleNextBlink();
            }
            else
            {
                // Smooth sine-like blinking curve
                float blinkFactor = Mathf.Sin(progress * Mathf.PI);
                float upperAngle = upperEyelidBlinkAngle * blinkFactor;
                float lowerAngle = lowerEyelidBlinkAngle * blinkFactor;

                // Eyelids are children of the eyes. To rotate them around the head's local horizontal axis (X in parent space),
                // we transform the parent-space blink rotation into the eye's local coordinate system:
                Quaternion leftUpperBlink = Quaternion.Inverse(leftEyeOriginalRot) * Quaternion.Euler(upperAngle, 0f, 0f) * leftEyeOriginalRot;
                Quaternion leftLowerBlink = Quaternion.Inverse(leftEyeOriginalRot) * Quaternion.Euler(lowerAngle, 0f, 0f) * leftEyeOriginalRot;

                Quaternion rightUpperBlink = Quaternion.Inverse(rightEyeOriginalRot) * Quaternion.Euler(upperAngle, 0f, 0f) * rightEyeOriginalRot;
                Quaternion rightLowerBlink = Quaternion.Inverse(rightEyeOriginalRot) * Quaternion.Euler(lowerAngle, 0f, 0f) * rightEyeOriginalRot;

                if (leftUpperEyelid != null)
                    leftUpperEyelid.localRotation = leftUpperBlink * leftUpperEyelidOriginalRot;
                if (leftLowerEyelid != null)
                    leftLowerEyelid.localRotation = leftLowerBlink * leftLowerEyelidOriginalRot;
                
                if (rightUpperEyelid != null)
                    rightUpperEyelid.localRotation = rightUpperBlink * rightUpperEyelidOriginalRot;
                if (rightLowerEyelid != null)
                    rightLowerEyelid.localRotation = rightLowerBlink * rightLowerEyelidOriginalRot;
            }
        }
        else if (Time.time >= nextBlinkTime)
        {
            isBlinking = true;
            blinkStartTime = Time.time;
        }
    }

    private void ProcessEyeMovements()
    {
        if (Time.time >= nextSaccadeTime)
        {
            targetYaw = Random.Range(-maxYawAngle, maxYawAngle);
            targetPitch = Random.Range(-maxPitchAngle, maxPitchAngle);
            
            ScheduleNextSaccade();
        }

        // Smoothly interpolate eye gaze angles
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, eyeSpeed * Time.deltaTime);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, eyeSpeed * Time.deltaTime);

        // Apply pitch and yaw in the parent's coordinate system (faceAttach space) by multiplying on the left
        Quaternion lookOffset = Quaternion.Euler(currentPitch, currentYaw, 0f);

        if (leftEye != null)
            leftEye.localRotation = lookOffset * leftEyeOriginalRot;
        if (rightEye != null)
            rightEye.localRotation = lookOffset * rightEyeOriginalRot;
    }

    private void ProcessJawBreathing()
    {
        if (jaw != null)
        {
            // Simulate breathing jaw movement
            float breathing = Mathf.Sin(Time.time * jawFrequency);
            float jawAngle = Mathf.Max(0f, breathing) * jawIdleMovement;
            
            // Apply jaw opening in parent space (rotation around horizontal X axis)
            jaw.localRotation = Quaternion.Euler(jawAngle, 0f, 0f) * jawOriginalRot;
        }
    }

    private void ProcessBrows()
    {
        // Eyebrows react to vertical eye movements (raising/lowering in parent space)
        float browOffset = currentPitch * browResponseToEyes;
        Quaternion browOffsetRot = Quaternion.Euler(browOffset, 0f, 0f);
        
        if (leftBrow != null)
            leftBrow.localRotation = browOffsetRot * leftBrowOriginalRot;
        if (rightBrow != null)
            rightBrow.localRotation = browOffsetRot * rightBrowOriginalRot;
        if (midBrow != null)
            midBrow.localRotation = Quaternion.Euler(browOffset * 0.5f, 0f, 0f) * midBrowOriginalRot;
    }

    private void ResetEyelids()
    {
        if (leftUpperEyelid != null) leftUpperEyelid.localRotation = leftUpperEyelidOriginalRot;
        if (leftLowerEyelid != null) leftLowerEyelid.localRotation = leftLowerEyelidOriginalRot;
        if (rightUpperEyelid != null) rightUpperEyelid.localRotation = rightUpperEyelidOriginalRot;
        if (rightLowerEyelid != null) rightLowerEyelid.localRotation = rightLowerEyelidOriginalRot;
    }

    private void ScheduleNextBlink()
    {
        nextBlinkTime = Time.time + Random.Range(blinkIntervalMin, blinkIntervalMax);
    }

    private void ScheduleNextSaccade()
    {
        nextSaccadeTime = Time.time + Random.Range(saccadeIntervalMin, saccadeIntervalMax);
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
