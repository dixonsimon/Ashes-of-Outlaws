using UnityEngine;

public class BanditFacialAnimator : MonoBehaviour
{
    [Header("Blink Settings")]
    public float blinkIntervalMin = 3.0f;
    public float blinkIntervalMax = 6.0f;
    public float blinkDuration = 0.15f;
    public Vector3 upperEyelidBlinkRot = new Vector3(30f, 0f, 0f);
    public Vector3 lowerEyelidBlinkRot = new Vector3(-8f, 0f, 0f);

    [Header("Eye Look Settings")]
    public float maxYawAngle = 10f;
    public float maxPitchAngle = 6f;
    public float saccadeIntervalMin = 1.5f;
    public float saccadeIntervalMax = 4.0f;
    public float eyeSpeed = 15f;

    [Header("Jaw & Brow Settings")]
    public float jawIdleMovement = 2.0f;
    public float jawFrequency = 1.2f;
    public float browResponseToEyes = 0.3f;

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

    private Quaternion targetEyeRot = Quaternion.identity;
    private Quaternion currentEyeRot = Quaternion.identity;
    private float nextSaccadeTime;
    private float currentEyePitch = 0f;

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
                
                if (leftUpperEyelid != null)
                    leftUpperEyelid.localRotation = leftUpperEyelidOriginalRot * Quaternion.Euler(upperEyelidBlinkRot * blinkFactor);
                if (rightUpperEyelid != null)
                    rightUpperEyelid.localRotation = rightUpperEyelidOriginalRot * Quaternion.Euler(upperEyelidBlinkRot * blinkFactor);
                
                if (leftLowerEyelid != null)
                    leftLowerEyelid.localRotation = leftLowerEyelidOriginalRot * Quaternion.Euler(lowerEyelidBlinkRot * blinkFactor);
                if (rightLowerEyelid != null)
                    rightLowerEyelid.localRotation = rightLowerEyelidOriginalRot * Quaternion.Euler(lowerEyelidBlinkRot * blinkFactor);
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
            float randomYaw = Random.Range(-maxYawAngle, maxYawAngle);
            float randomPitch = Random.Range(-maxPitchAngle, maxPitchAngle);
            
            // Keep track of current vertical offset to animate brows later
            currentEyePitch = randomPitch;

            targetEyeRot = Quaternion.Euler(randomPitch, randomYaw, 0f);
            ScheduleNextSaccade();
        }

        // Smoothly interpolate eye gaze rotation
        currentEyeRot = Quaternion.Slerp(currentEyeRot, targetEyeRot, eyeSpeed * Time.deltaTime);

        if (leftEye != null)
            leftEye.localRotation = leftEyeOriginalRot * currentEyeRot;
        if (rightEye != null)
            rightEye.localRotation = rightEyeOriginalRot * currentEyeRot;
    }

    private void ProcessJawBreathing()
    {
        if (jaw != null)
        {
            // Simulate breathing jaw movement
            float breathing = Mathf.Sin(Time.time * jawFrequency);
            float jawAngle = Mathf.Max(0f, breathing) * jawIdleMovement;
            
            jaw.localRotation = jawOriginalRot * Quaternion.Euler(jawAngle, 0f, 0f);
        }
    }

    private void ProcessBrows()
    {
        // Eyebrows react to vertical eye movements (saccades)
        float browOffset = currentEyePitch * browResponseToEyes;
        
        if (leftBrow != null)
            leftBrow.localRotation = leftBrowOriginalRot * Quaternion.Euler(0f, 0f, -browOffset);
        if (rightBrow != null)
            rightBrow.localRotation = rightBrowOriginalRot * Quaternion.Euler(0f, 0f, browOffset);
        if (midBrow != null)
            midBrow.localRotation = midBrowOriginalRot * Quaternion.Euler(0f, 0f, -browOffset * 0.5f);
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
