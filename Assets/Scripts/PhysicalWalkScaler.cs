using UnityEngine;
using System.Collections.Generic;

public class PhysicalWalkScaler : MonoBehaviour
{
    public Transform vrCamera;
    public Transform playerRoot;
    public float defaultScaleFactor = 1.2f;
    public bool includeYMovement = false;

    [Header("Running")]
    public bool enableRunningDetection = true;
    public float runningScaleFactor = 5f;
    public Transform leftController;
    public Transform rightController;
    public float runningDetectionThreshold = 1f;
    public const int runningDetectionFrames = 30;
    public float scaleAccelerationSpeed = 3f; 
    public float scaleDecelerationSpeed = 10f;

    private Vector3 lastHeadsetPosition;
    private float currentAppliedScaleFactor;

    private const int CONTROLLER_HISTORY_SIZE = runningDetectionFrames;
    private List<Vector3> leftControllerLocalPositionHistory = new List<Vector3>();
    private List<Vector3> rightControllerLocalPositionHistory = new List<Vector3>();
    
    public bool IsRunning { get; private set; }

    void Start()
    {
        currentAppliedScaleFactor = defaultScaleFactor;

        if (leftController == null || rightController == null || vrCamera == null || playerRoot == null)
        {
            Debug.LogError("Missing required references!");
        }
        else
        {
            // Initialize history to prevent errors on first few frames
            for (int i = 0; i < CONTROLLER_HISTORY_SIZE; i++)
            {
                leftControllerLocalPositionHistory.Add(leftController.localPosition);
                rightControllerLocalPositionHistory.Add(rightController.localPosition);
            }
        }

        if (!enableRunningDetection)
        {
            IsRunning = false;
        }

        lastHeadsetPosition = vrCamera.localPosition;
    }

    void LateUpdate()
    {
        if (enableRunningDetection && leftController != null && rightController != null)
        {
            UpdateControllerHistory();
            CheckForRunning();
        }
        else
        {
            IsRunning = false;
        }

        // Calculate delta movement
        Vector3 currentHeadsetPosition = vrCamera.localPosition;
        Vector3 delta = currentHeadsetPosition - lastHeadsetPosition;

        float targetScaleFactor = IsRunning ? runningScaleFactor : defaultScaleFactor;
        
        // Interpolate scale factor
        float currentInterpolationSpeed;
        if (targetScaleFactor > currentAppliedScaleFactor)
        {
            currentInterpolationSpeed = scaleAccelerationSpeed; // Speeding up
        }
        else
        {
            currentInterpolationSpeed = scaleDecelerationSpeed; // Slowing down
        }
        
        currentAppliedScaleFactor = Mathf.Lerp(currentAppliedScaleFactor, targetScaleFactor, Time.deltaTime * currentInterpolationSpeed);

        // Scale the movement
        Vector3 scaledDelta = Vector3.zero;
        if (!includeYMovement)
        {
            scaledDelta.x = delta.x * currentAppliedScaleFactor;
            scaledDelta.y = delta.y;
            scaledDelta.z = delta.z * currentAppliedScaleFactor;
        }
        else
        {
            scaledDelta = delta * currentAppliedScaleFactor;
        }

        playerRoot.position += playerRoot.TransformDirection(scaledDelta);


        lastHeadsetPosition = currentHeadsetPosition;
    }

    /// <summary>
    /// Updates the local position history of the controllers.
    void UpdateControllerHistory()
    {
        leftControllerLocalPositionHistory.Add(leftController.localPosition);
        rightControllerLocalPositionHistory.Add(rightController.localPosition);

        // Remove oldest positions if history size too large
        while (leftControllerLocalPositionHistory.Count > CONTROLLER_HISTORY_SIZE)
        {
            leftControllerLocalPositionHistory.RemoveAt(0);
        }
        while (rightControllerLocalPositionHistory.Count > CONTROLLER_HISTORY_SIZE)
        {
            rightControllerLocalPositionHistory.RemoveAt(0);
        }
    }

    /// <summary>
    /// Checks if the user is running based on the controller movement history
    void CheckForRunning()
    {
        if (leftControllerLocalPositionHistory.Count < CONTROLLER_HISTORY_SIZE || 
            rightControllerLocalPositionHistory.Count < CONTROLLER_HISTORY_SIZE)
        {
            IsRunning = false;
            return;
        }

        float leftMovementSum = 0f;
        float rightMovementSum = 0f;

        // Calculate total movement for both controllers
        for (int i = 1; i < CONTROLLER_HISTORY_SIZE; i++)
        {
            leftMovementSum += Vector3.Distance(leftControllerLocalPositionHistory[i], leftControllerLocalPositionHistory[i - 1]);
            rightMovementSum += Vector3.Distance(rightControllerLocalPositionHistory[i], rightControllerLocalPositionHistory[i - 1]);
        }

        float totalControllerMovement = leftMovementSum + rightMovementSum;
        
        IsRunning = totalControllerMovement > runningDetectionThreshold;

    }
}
