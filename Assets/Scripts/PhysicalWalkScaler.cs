using UnityEngine;
using System.Collections.Generic;

public class PhysicalWalkScaler : MonoBehaviour
{
    [Header("Walking")]
    public Transform vrCamera;
    public Transform playerRoot;
    public float defaultScaleFactor = 1.2f; // Default walking scale factor | Im spiel: 1.2f (20%)
    public bool includeYMovement = false; // Include Y-axis movement in scaling (up/down)

    [Header("Running")]
    public Transform leftController;
    public Transform rightController;
    public bool enableRunningDetection = true; // Enable running
    public float runningScaleFactor = 5f; // Running scale factor | Im spiel: ca. 14.0f
    public float runningDetectionThreshold = 1f; // When the total controller movement exceeds this threshold, running is detected | Im spiel: 1.1f
    public const int runningDetectionFrames = 30; // Number of frames to consider for running detection
    public float scaleAccelerationSpeed = 3f; // Im spiel: 1.0f
    public float scaleDecelerationSpeed = 10f; // Im spiel: 16.0f


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
            Debug.LogError("Missing references!");
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
        if (enableRunningDetection)
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

        // Interpolates the current scale factor towards running or walking
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

        // Apply scaled movement to the player
        playerRoot.position += playerRoot.TransformDirection(scaledDelta);

        // Update headset position for next frame
        lastHeadsetPosition = currentHeadsetPosition;
    }

    // Update the local position history of the controllers
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

    // Checks if the user is running based on the controller movement history
    void CheckForRunning()
    {
        // Return if not enough positions in history
        if (leftControllerLocalPositionHistory.Count < CONTROLLER_HISTORY_SIZE ||
            rightControllerLocalPositionHistory.Count < CONTROLLER_HISTORY_SIZE)
        {
            IsRunning = false;
            return;
        }

        float leftMovementSum = 0f;
        float rightMovementSum = 0f;

        // Calculate total movement for both controllers
        // For each frame, calculate the distance between the current and previous position
        for (int i = 1; i < CONTROLLER_HISTORY_SIZE; i++)
        {
            // Calculate distance between current and previous position
            leftMovementSum += Vector3.Distance(leftControllerLocalPositionHistory[i], leftControllerLocalPositionHistory[i - 1]);
            rightMovementSum += Vector3.Distance(rightControllerLocalPositionHistory[i], rightControllerLocalPositionHistory[i - 1]);
        }

        float totalControllerMovement = leftMovementSum + rightMovementSum;
        
        IsRunning = totalControllerMovement > runningDetectionThreshold;

    }
}
