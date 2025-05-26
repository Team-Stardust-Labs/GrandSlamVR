// StartupScript.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupScript : MonoBehaviour
{
    public string configKey = "PlayerColor";
    public string configSceneName = "ConfigScene";
    public string gameplaySceneName = "FinalMapScene";

    public static readonly string RunModePlayerPrefKey = "RunMode";
    public static readonly string VrModeValue = "VR";
    public static readonly string SpectatorModeValue = "Spectator";

    void Start()
    {
        Debug.Log("StartupScript: Start() wurde aufgerufen."); // Log 1

        if (RunModePlayerPrefKey == null)
        {
            Debug.LogError("StartupScript: FEHLER - RunModePlayerPrefKey ist NULL!");
            // Hier könntest du eine Notfallaktion ausführen oder das Spiel anhalten
            return; // Verhindere weiteren Code, der crashen würde
        }
        Debug.Log($"StartupScript: RunModePlayerPrefKey hat den Wert: '{RunModePlayerPrefKey}'"); // Log 2

        // Versuche, den PlayerPref zu lesen
        string currentRunMode = ""; // Initialisiere mit einem leeren String
        try
        {
            currentRunMode = PlayerPrefs.GetString(RunModePlayerPrefKey);
            Debug.Log($"StartupScript: PlayerPrefs.GetString erfolgreich. currentRunMode ist: '{currentRunMode}'"); // Log 3
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"StartupScript: FEHLER beim Lesen von PlayerPrefs mit Key '{RunModePlayerPrefKey}': {ex.ToString()}");
            // Fallback, wenn das Lesen fehlschlägt
            currentRunMode = SpectatorModeValue; // Gehe von Spectator aus, um weiterzumachen
            Debug.LogWarning("StartupScript: Fallback zu SpectatorModeValue aufgrund eines Fehlers beim Lesen der PlayerPrefs.");
        }


        if (currentRunMode == VrModeValue)
        {
            Debug.Log($"StartupScript: Modus ist VR. Prüfe {configKey}.");
            if (PlayerPrefs.HasKey(configKey))
            {
                SceneManager.LoadScene(gameplaySceneName);
            }
            else
            {
                SceneManager.LoadScene(configSceneName);
            }
        }
        else if (currentRunMode == SpectatorModeValue)
        {
            Debug.Log("StartupScript: Modus ist Spectator. Lade Gameplay-Szene.");
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            Debug.LogWarning($"StartupScript: Unbekannter oder leerer RunMode: '{currentRunMode}'. Fallback zu Spectator und lade Gameplay-Szene.");
            // Im Falle eines leeren oder unerwarteten Strings, gehe sicherheitshalber in den Spectator-Modus
            // (oder eine andere definierte Startlogik)
            PlayerPrefs.SetString(RunModePlayerPrefKey, SpectatorModeValue); // Setze es explizit für diesen Fall
            PlayerPrefs.Save();
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}