// StartupScript.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupScript : MonoBehaviour
{
    public string configKey = "PlayerColor"; // Preference for storing player color
    public string configSceneName = "ConfigScene"; // Scene name for configuration
    public string gameplaySceneName = "FinalMapScene"; // Scene name for gameplay

    public static readonly string RunModePlayerPrefKey = "RunMode"; // Key for PlayerPrefs to store the run mode

    // Readonly values for run modes
    public static readonly string VrModeValue = "VR";
    public static readonly string SpectatorModeValue = "Spectator";

    void Start()
    {
        // Catches invalid state of no RunMode
        if (RunModePlayerPrefKey == null)
        {
            return;
        }

        // Reads run mode
        string currentRunMode = "Spectator"; // Default run mode is spectator
        try
        {
            currentRunMode = PlayerPrefs.GetString(RunModePlayerPrefKey);
        }
        catch (System.Exception ex)
        {
            currentRunMode = SpectatorModeValue; // Fallback to spectator
        }

        // VR Mode loads into gameplay or configuration scene based on PlayerPrefs
        if (currentRunMode == VrModeValue)
        {
            // Player already has a color selected, load gameplay scene
            if (PlayerPrefs.HasKey(configKey))
            {
                SceneManager.LoadScene(gameplaySceneName);
            }
            // Player needs to select a color before playing, load configuration scene
            else
            {
                SceneManager.LoadScene(configSceneName);
            }
        }
        // Spectator Mode loads directly into gameplay scene
        else if (currentRunMode == SpectatorModeValue)
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        // No run Mode falls back to Spectator Mode and loads gameplay scene
        else
        {
            PlayerPrefs.SetString(RunModePlayerPrefKey, SpectatorModeValue);
            PlayerPrefs.Save();
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}