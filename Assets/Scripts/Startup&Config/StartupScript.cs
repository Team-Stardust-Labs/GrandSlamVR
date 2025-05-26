// StartupScript.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupScript : MonoBehaviour
{
    public string configKey = "PlayerColor";
    public string configSceneName = "ConfigScene";
    public string gameplaySceneName = "FinalMapScene";

    // Diese Konstanten müssen hier bekannt sein, oder aus einer gemeinsamen Datei kommen
    public static readonly string RunModePlayerPrefKey = "RunMode";
    public static readonly string VrModeValue = "VR";
    public static readonly string SpectatorModeValue = "Spectator";

    void Start()
    {
        // CustomDebugLog.Singleton.Log("StartupScript: Start() aufgerufen.");
        string currentRunMode = PlayerPrefs.GetString(RunModePlayerPrefKey);
        CustomDebugLog.Singleton.Log($"StartupScript: Aktueller RunMode aus PlayerPrefs: {currentRunMode}");

        if (currentRunMode == VrModeValue)
        {
            // CustomDebugLog.Singleton.Log($"StartupScript: Modus ist VR. Prüfe {configKey}.");
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
            // CustomDebugLog.Singleton.Log("StartupScript: Modus ist Spectator. Lade Gameplay-Szene.");
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            // Fallback, falls RunMode nicht gesetzt oder ungültig ist
            // CustomDebugLog.Singleton.LogWarning($"StartupScript: Unbekannter RunMode: {currentRunMode}. Gehe von Spectator aus.");
            SceneManager.LoadScene(gameplaySceneName); // Oder eine Fehlerbehandlung
        }
    }
}