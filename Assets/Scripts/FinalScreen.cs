using UnityEngine;

/// <summary>
/// Handles the display and positioning of the end screen UI when the game finishes.
/// Shows which player has won and positions the canvas in front of the main camera.
/// </summary>
public class FinalScreen : MonoBehaviour
{
    public GameObject endScreenCanvas; // The main end screen canvas object
    public GameObject bluewin;         // UI element shown if blue wins
    public GameObject redwin;          // UI element shown if red wins
    public SpectatorManager spec;      // Reference to the spectator manager
    public ScoreManager scoreManager;  // Reference to the score manager

    private float distance = 7f;       // Distance in front of the camera to place the end screen
    private float endScreenScale = 0.6f; // Scale factor for the end screen

    void Start()
    {
        // Hide all end screen UI elements at the start
        endScreenCanvas.SetActive(false);
        bluewin.SetActive(false);
        redwin.SetActive(false);
    }

    void Update()
    {
        // Check if the game is finished
        if (ScoreManager.isGameFinished())
        {
            // Only show the end screen for players, not spectators
            if (!SpectatorManager.isSpectator())
            {
                showEndScreenPlayer();
            }
            else
            {
                // Optionally add a seperate spectator end screen here
                showEndScreenPlayer();
            }
        }
    }

    /// Shows the end screen for the player, positions and scales it in front of the camera,
    /// and displays the correct winner.
    void showEndScreenPlayer()
    {
        Camera cam = Camera.main;

        endScreenCanvas.SetActive(true);

        // Show the correct winner UI
        if (scoreManager.isBlueWinner())
        {
            bluewin.SetActive(true);
        }
        else
        {
            redwin.SetActive(true);
        }

        // Calculate the size of the canvas based on camera FOV and distance
        float height = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * endScreenScale;
        float width = height * cam.aspect;

        // Reference resolution for scaling
        float referenceWidth = 1920f;
        float referenceHeight = 1080f;

        float scaleX = width / referenceWidth;
        float scaleY = height / referenceHeight;
        float scale = Mathf.Min(scaleX, scaleY);

        // Scale the canvas to fit the view
        endScreenCanvas.transform.localScale = Vector3.one * scale;

        // Position the canvas in front of the camera
        Vector3 forward = cam.transform.forward;
        Vector3 spawnPosition = cam.transform.position + forward * distance;

        // Ensure the canvas is not below the floor
        spawnPosition.y = Mathf.Max(spawnPosition.y, 2f);

        // Set the position of the end screen canvas
        endScreenCanvas.transform.position = spawnPosition;

        // Make the canvas face the camera
        endScreenCanvas.transform.LookAt(cam.transform);
        endScreenCanvas.transform.Rotate(0, 180, 0);
    }
}
