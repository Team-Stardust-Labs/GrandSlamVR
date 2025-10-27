using Unity.Cinemachine;
using UnityEngine;

// Script that handles the Camera Switching functionality in the game.
public class CameraSwitching : MonoBehaviour
{
    public CinemachineCamera scoreboardCamera; // Camera that shows the scoreboard, will be shown on point score
    public CinemachineCamera[] cameras; // Array of cameras to switch between
    public float switchInterval; // regular time between switching cameras

    public int fastSlots; // first n entries of the cameras Array are to be switched out faster than the rest
    public float fastInterval; // time between switching cameras in the fast slots

    private float timer; // Timer to track the time until the next camera switch

    void Start()
    {
        timer = switchInterval; // Initialize the timer with the switch interval

        foreach (var cam in cameras) // set all cameras to default priority
            cam.Priority = 0;

        SwitchToScoreboard(); // Start with the scoreboard camera active
    }

    void Update()
    {
        timer -= Time.deltaTime; // Decrease the timer by the time since the last frame

        // Check if it is time to switch cameras
        if (timer <= 0f)
        {
            timer = switchInterval; // Reset the timer to the switch interval
            scoreboardCamera.Priority = 0; // Reset the scoreboard camera priority
            SwitchToRandomCamera(); // Call the switching method

        }
    }

    // Switches to a random camera from the cameras array.
    public void SwitchToRandomCamera()
    {
        int rng = UnityEngine.Random.Range(0, cameras.Length); // Generate a random index within the range of cameras array length
        SwitchTo(rng); // Call the SwitchTo method with the randomized index
    }

    // Switches to a specific camera by name per string (also overloaded method for index)
    public void SwitchTo(string cameraName)
    {
        foreach (var cam in cameras)
        {
            if (cam.name == cameraName)
            {
                cam.Priority = 10;
            }
            else
            {
                cam.Priority = 0;
            }
        }
    }

    // Overloaded method to switch to a specific camera by index
    public void SwitchTo(int cameraIndex)
    {
        // Set the priority of the specified camera to 10, and all others to 0 to activate it for Cinemachine
        for (int i = 0; i < cameras.Length; i++)
        {
            if (i == cameraIndex)
                cameras[i].Priority = 10;
            else
                cameras[i].Priority = 0;
        }
        // In case the cameraIndex is within the fast slots, set the timer to the fast interval instead
        if (cameraIndex <= fastSlots - 1)
        {
            timer = fastInterval;
        }
    }

    // Switches to the scoreboard camera, setting its priority higher than the others.
    protected void SwitchToScoreboard()
    {
        scoreboardCamera.Priority = 20; // Scoreboard priority to 20
        foreach (var cam in cameras) // set all other cameras to default priority
            cam.Priority = 0;
        timer = 1.0f;
    }

    // Method to be called when the score changes, switches to the scoreboard camera.
    public void OnScoreChanged()
    {
        SwitchToScoreboard();
    }
}
