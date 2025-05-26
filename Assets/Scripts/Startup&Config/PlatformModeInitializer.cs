// PlatformModeInitializer.cs
using UnityEngine;

public class PlatformModeInitializer : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("PlatformModeInitializer: Awake() wurde aufgerufen.");

        string targetRunMode = ""; // Variable, um zu sehen, was gesetzt werden soll

        #if UNITY_ANDROID
            Debug.Log("PlatformModeInitializer: UNITY_ANDROID ist definiert.");
            targetRunMode = StartupScript.VrModeValue;
            PlayerPrefs.SetString(StartupScript.RunModePlayerPrefKey, StartupScript.VrModeValue);
            Debug.Log($"PlatformModeInitializer: Android-Build. PlayerPrefs '{StartupScript.RunModePlayerPrefKey}' gesetzt auf: '{StartupScript.VrModeValue}'");
        #elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            Debug.Log("PlatformModeInitializer: UNITY_STANDALONE ist definiert.");
            targetRunMode = StartupScript.SpectatorModeValue;
            PlayerPrefs.SetString(StartupScript.RunModePlayerPrefKey, StartupScript.SpectatorModeValue);
            Debug.Log($"PlatformModeInitializer: PC-Standalone-Build. PlayerPrefs '{StartupScript.RunModePlayerPrefKey}' gesetzt auf: '{StartupScript.SpectatorModeValue}'");
        #else
            Debug.Log("PlatformModeInitializer: Weder UNITY_ANDROID noch UNITY_STANDALONE sind definiert (wahrscheinlich Editor oder andere Plattform).");
            bool isEditorVREnabled = UnityEngine.XR.XRSettings.isDeviceActive;
            Debug.Log($"PlatformModeInitializer: Editor - isEditorVREnabled: {isEditorVREnabled}, Application.isEditor: {Application.isEditor}");
            if (isEditorVREnabled && Application.isEditor)
            {
                targetRunMode = StartupScript.VrModeValue;
                PlayerPrefs.SetString(StartupScript.RunModePlayerPrefKey, StartupScript.VrModeValue);
                Debug.Log($"PlatformModeInitializer: Editor mit VR. PlayerPrefs '{StartupScript.RunModePlayerPrefKey}' gesetzt auf: '{StartupScript.VrModeValue}'");
            }
            else
            {
                targetRunMode = StartupScript.SpectatorModeValue;
                PlayerPrefs.SetString(StartupScript.RunModePlayerPrefKey, StartupScript.SpectatorModeValue);
                Debug.Log($"PlatformModeInitializer: Editor ohne VR (oder Fallback). PlayerPrefs '{StartupScript.RunModePlayerPrefKey}' gesetzt auf: '{StartupScript.SpectatorModeValue}'");
            }
        #endif

        // Zusätzliche Überprüfung, ob die Keys selbst null sind (sollte nicht passieren mit static readonly)
        if (StartupScript.RunModePlayerPrefKey == null) {
            Debug.LogError("PlatformModeInitializer: FEHLER - StartupScript.RunModePlayerPrefKey ist NULL!");
        }
        if (targetRunMode == null) { // targetRunMode sollte hier nie null sein, da es immer zugewiesen wird
             Debug.LogError("PlatformModeInitializer: FEHLER - targetRunMode ist NULL! Das sollte nicht passieren.");
        }

        PlayerPrefs.Save();
        Debug.Log("PlatformModeInitializer: PlayerPrefs.Save() aufgerufen.");

        // Testweise direkt nach dem Speichern wieder auslesen, um sicherzustellen, dass es geschrieben wurde
        if (PlayerPrefs.HasKey(StartupScript.RunModePlayerPrefKey))
        {
            string writtenValue = PlayerPrefs.GetString(StartupScript.RunModePlayerPrefKey);
            Debug.Log($"PlatformModeInitializer: Wert nach Save() aus PlayerPrefs gelesen: '{writtenValue}' (Sollte '{targetRunMode}' sein)");
        }
        else
        {
            Debug.LogError($"PlatformModeInitializer: FEHLER - Key '{StartupScript.RunModePlayerPrefKey}' wurde nach Save() nicht in PlayerPrefs gefunden!");
        }
    }
}