// PlatformModeInitializer.cs
using UnityEngine;

public class PlatformModeInitializer : MonoBehaviour
{
    void Awake() // Awake wird sehr früh ausgeführt, noch vor Start()
    {
        // CustomDebugLog.Singleton.Log("PlatformModeInitializer: Awake() aufgerufen.");

        #if UNITY_ANDROID
            PlayerPrefs.SetString(StartupScript.RunModePlayerPrefKey, StartupScript.VrModeValue);
            // CustomDebugLog.Singleton.Log($"PlatformModeInitializer: Android-Build. RunMode gesetzt auf: {StartupScript.VrModeValue}");
        #elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            PlayerPrefs.SetString(StartupScript.RunModePlayerPrefKey, StartupScript.SpectatorModeValue);
            // CustomDebugLog.Singleton.Log($"PlatformModeInitializer: PC-Standalone-Build. RunMode gesetzt auf: {StartupScript.SpectatorModeValue}");
        #else
            // Editor oder andere Plattformen
            // CustomDebugLog.Singleton.Log("PlatformModeInitializer: Editor oder unbekannte Plattform.");
            bool isEditorVREnabled = UnityEngine.XR.XRSettings.isDeviceActive;
            if (isEditorVREnabled && Application.isEditor)
            {
                PlayerPrefs.SetString(StartupScript.RunModePlayerPrefKey, StartupScript.VrModeValue);
                // CustomDebugLog.Singleton.Log($"PlatformModeInitializer: Editor mit VR. RunMode: {StartupScript.VrModeValue}");
            }
            else
            {
                PlayerPrefs.SetString(StartupScript.RunModePlayerPrefKey, StartupScript.SpectatorModeValue);
                // CustomDebugLog.Singleton.Log($"PlatformModeInitializer: Editor ohne VR. RunMode: {StartupScript.SpectatorModeValue}");
            }
        #endif
        PlayerPrefs.Save();
        // CustomDebugLog.Singleton.Log("PlatformModeInitializer: PlayerPrefs gespeichert.");
    }
}