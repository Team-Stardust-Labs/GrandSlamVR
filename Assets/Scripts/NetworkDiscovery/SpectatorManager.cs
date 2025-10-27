/*
Overview:
SpectatorManager handles spectator mode in an XR Netcode application. It:
  • Detects if the current session is spectator based on PlayerPrefs
  • Activates a dedicated spectator camera and disables player spawning
  • Starts and stops LAN discovery for auto-joining as a spectator
  • Provides a hotkey to return to the startup scene and stop listening
*/

using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections; // For NetworkManager und NetworkConfig

public class SpectatorManager : MonoBehaviour
{
    [Header("Scene GameObjects - Im Inspector zuweisen!")]
    public GameObject spectatorCameraObject; // Camera used for spectator mode
    public GameObject joinDiscoveryObject;   // Object with LanDiscoveryClient & ConnectToDiscoveredHost scripts

    private ConnectToDiscoveredHost lanConnector; // Reference to connection script
    private bool isActiveAndAttemptingJoin = false; // Prevents redundant join attempts

    void Awake()
    {
        // Cache the connector component if assigned
        if (joinDiscoveryObject != null)
        {
            lanConnector = joinDiscoveryObject.GetComponent<ConnectToDiscoveredHost>();
        }
    }

    // Determines if the current run mode equals spectator
    public static bool isSpectator()
    {
        string currentRunMode = PlayerPrefs.GetString(StartupScript.RunModePlayerPrefKey);
        return currentRunMode == StartupScript.SpectatorModeValue;
    }

    void Start()
    {
        string currentRunMode = PlayerPrefs.GetString(StartupScript.RunModePlayerPrefKey);

        if (currentRunMode == StartupScript.SpectatorModeValue)
        {
            InitializeSpectatorMode(); // Enable spectator functionality
        }
        else
        {
            // Disable if not in spectator mode
            if (spectatorCameraObject != null)
                spectatorCameraObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Press 'R' to stop discovery and reload startup scene
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (joinDiscoveryObject != null)
            {
                lanConnector.discovery.StopListening();
            }
            SceneManager.LoadScene("StartupScene");
        }
    }

    // Configures the scene and network for a spectator
    void InitializeSpectatorMode()
    {
        if (spectatorCameraObject != null)
        {
            spectatorCameraObject.SetActive(true);    // Switch to spectator camera
            Camera.main.tag = "Untagged";            // Remove tag from existing camera
            spectatorCameraObject.tag = "MainCamera"; // Tag spectator camera for UI
        }

        // Prevent spawning a player avatar
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;
        }

        // Start LAN discovery if not already connected
        if (joinDiscoveryObject != null && lanConnector != null)
        {
            bool notConnected = NetworkManager.Singleton == null
                                || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer);
            if (notConnected)
            {
                joinDiscoveryObject.SetActive(true);
                InvokeRepeating(nameof(AttemptToJoinDiscoveredHost), 2.0f, 2.0f);
                isActiveAndAttemptingJoin = true;
            }
        }
    }

    // Tries to auto-join a discovered host once an address is found
    void AttemptToJoinDiscoveredHost()
    {
        if (!isActiveAndAttemptingJoin)
            return;

        if (lanConnector != null && lanConnector.discovery != null && !string.IsNullOrEmpty(lanConnector.discovery.foundAddress))
        {
            lanConnector.TryConnect();            // Initiate Netcode client connection
            CancelInvoke(nameof(AttemptToJoinDiscoveredHost));
            isActiveAndAttemptingJoin = false;   // Stop further attempts
        }
    }

    void OnDestroy()
    {
        // Ensure any pending invokes are canceled
        CancelInvoke(nameof(AttemptToJoinDiscoveredHost));
    }
}
