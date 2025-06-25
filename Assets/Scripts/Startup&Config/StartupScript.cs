// StartupScript.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupScript : MonoBehaviour
{
    public string configKey = "PlayerColor"; // Preference for storing player color
    public string configSceneName = "ConfigScene"; // Scene name for configuration
    public string gameplaySceneName = "FinalMapScene"; // Scene name for gameplay

    public static readonly string RunModePlayerPrefKey = "RunMode"; // Key for PlayerPrefs to store the run mode
    public static readonly string VrModeValue = "VR";
    public static readonly string SpectatorModeValue = "Spectator";

    void Start()
    {
        Debug.Log("StartupScript: Start() wurde aufgerufen."); // Log 1

        // Catches invalid state
        if (RunModePlayerPrefKey == null)
        {
            Debug.LogError("StartupScript: FEHLER - RunModePlayerPrefKey ist NULL!");
            return;
        }
        Debug.Log($"StartupScript: RunModePlayerPrefKey hat den Wert: '{RunModePlayerPrefKey}'"); // Log 2

        // Reads run mode
        string currentRunMode = "Spectator"; // Default run mode is spectator
        try
        {
            currentRunMode = PlayerPrefs.GetString(RunModePlayerPrefKey);
            Debug.Log($"StartupScript: PlayerPrefs.GetString erfolgreich. currentRunMode ist: '{currentRunMode}'"); // Log 3
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"StartupScript: FEHLER beim Lesen von PlayerPrefs mit Key '{RunModePlayerPrefKey}': {ex.ToString()}");
            currentRunMode = SpectatorModeValue; // Fallback to spectator
            Debug.LogWarning("StartupScript: Fallback zu SpectatorModeValue aufgrund eines Fehlers beim Lesen der PlayerPrefs.");
        }

        // VR Mode loads into gameplay or configuration scene based on PlayerPrefs
        if (currentRunMode == VrModeValue)
        {
            Debug.Log($"StartupScript: Modus ist VR. Prüfe {configKey}.");
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
            Debug.Log("StartupScript: Modus ist Spectator. Lade Gameplay-Szene.");
            SceneManager.LoadScene(gameplaySceneName);
        }
        // No run Mode falls back to Spectator Mode and loads gameplay scene
        else
        {
            Debug.LogWarning($"StartupScript: Unbekannter oder leerer RunMode: '{currentRunMode}'. Fallback zu Spectator und lade Gameplay-Szene.");
            PlayerPrefs.SetString(RunModePlayerPrefKey, SpectatorModeValue);
            PlayerPrefs.Save();
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}