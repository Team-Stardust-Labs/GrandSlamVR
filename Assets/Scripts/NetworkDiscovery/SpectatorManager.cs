// SpectatorManager.cs
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
    private bool isActiveAndAttemptingJoin = false; // Prevents multiple join attempts after success

    void Awake()
    {
        // Get reference to the LAN connector script
        if (joinDiscoveryObject != null)
        {
            lanConnector = joinDiscoveryObject.GetComponent<ConnectToDiscoveredHost>();
            if (lanConnector == null)
            {
                Debug.LogError("SpectatorManager: ConnectToDiscoveredHost script not found on joinDiscoveryObject!");
            }
        }
        else
        {
            Debug.LogWarning("SpectatorManager: joinDiscoveryObject not assigned in Inspector!");
        }
    }

    // Returns true if the current run mode is spectator
    public static bool isSpectator()
    {
        string currentRunMode = PlayerPrefs.GetString(StartupScript.RunModePlayerPrefKey);

        if (currentRunMode == StartupScript.SpectatorModeValue)
        {
            return true;
        }
        return false;

        return currentRunMode == StartupScript.SpectatorModeValue;
    }

    void Start()
    {
        // Check run mode and initialize spectator mode if needed
        string currentRunMode = PlayerPrefs.GetString(StartupScript.RunModePlayerPrefKey);

        if (currentRunMode == StartupScript.SpectatorModeValue)
        {
            InitializeSpectatorMode();
        }
        else
        {
            // Disable spectator camera and this GameObject if not in spectator mode
            if (spectatorCameraObject != null)
                spectatorCameraObject.SetActive(false);

            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Press 'R' to stop discovery and return to startup scene to reload everything
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (joinDiscoveryObject != null)
            {
                lanConnector.discovery.StopListening();
            }
            SceneManager.LoadScene("StartupScene");
        }
    }

    // Sets up spectator mode: camera, disables player prefab, starts LAN discovery
    void InitializeSpectatorMode()
    {
        if (spectatorCameraObject != null)
        {
            spectatorCameraObject.SetActive(true);
            Camera.main.tag = "Untagged"; // Removes main Camera tag from the default camera
            spectatorCameraObject.tag = "MainCamera"; // Sets main Camera to Spectator Camera, important for EndScreen UI
        }
        else
        {
            Debug.LogError("SpectatorManager: spectatorCameraObject not assigned!");
        }

        // Prevent spawning a player object for the spectator
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;
        }
        else
        {
            Debug.LogError("SpectatorManager: NetworkManager or NetworkConfig not found!");
        }

        // Start LAN discovery and auto-join attempts
        if (joinDiscoveryObject != null && lanConnector != null)
        {
            if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer))
            {
                joinDiscoveryObject.SetActive(true);
                InvokeRepeating(nameof(AttemptToJoinDiscoveredHost), 2.0f, 2.0f);
                isActiveAndAttemptingJoin = true;
            }
        }
        else
        {
            Debug.LogError("SpectatorManager: joinDiscoveryObject or lanConnector not initialized. Cannot start auto-join.");
        }
    }

    // Tries to connect to a discovered host if one is found
    void AttemptToJoinDiscoveredHost()
    {
        // Check if we are still active and attempting to join to skip unnecessary attempts
        if (!isActiveAndAttemptingJoin)
            return;

        if (lanConnector != null && lanConnector.discovery != null && !string.IsNullOrEmpty(lanConnector.discovery.foundAddress))
        {
            lanConnector.TryConnect(); // Should call NetworkManager.Singleton.StartClient()
            CancelInvoke(nameof(AttemptToJoinDiscoveredHost));
            isActiveAndAttemptingJoin = false;
        }
    }

    void OnDestroy()
    {
        // Ensure InvokeRepeating is stopped when this object is destroyed
        CancelInvoke(nameof(AttemptToJoinDiscoveredHost));
    }
}
