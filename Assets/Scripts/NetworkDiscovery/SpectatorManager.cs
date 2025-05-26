// SpectatorManager.cs
using UnityEngine;
using Unity.Netcode; // Für NetworkManager und NetworkConfig

public class SpectatorManager : MonoBehaviour
{
    [Header("Scene GameObjects - Im Inspector zuweisen!")]
    public GameObject spectatorCameraObject; // Die Kamera für den Spectator-Modus
    public GameObject joinDiscoveryObject;   // Das GameObject mit LanDiscoveryClient & ConnectToDiscoveredHost Skripten

    private ConnectToDiscoveredHost lanConnector; // Referenz zum Skript für den Verbindungsaufbau
    private bool isActiveAndAttemptingJoin = false; // Flag, um mehrfache Join-Versuche nach Erfolg zu vermeiden

    void Awake()
    {
        Debug.Log("SpectatorManager: Awake() aufgerufen.");
        // Hole die Referenz zum LanConnector-Skript auf dem zugewiesenen joinDiscoveryObject
        if (joinDiscoveryObject != null)
        {
            lanConnector = joinDiscoveryObject.GetComponent<ConnectToDiscoveredHost>();
            if (lanConnector == null)
            {
                Debug.LogError("SpectatorManager: ConnectToDiscoveredHost Skript nicht auf joinDiscoveryObject gefunden!");
            }
            else
            {
                Debug.Log("SpectatorManager: LanConnector erfolgreich gefunden.");
            }
        }
        else
        {
            Debug.LogWarning("SpectatorManager: joinDiscoveryObject ist nicht im Inspector zugewiesen!");
        }
    }

    void Start()
    {
        Debug.Log("SpectatorManager: Start() aufgerufen.");
        // Lese den aktuellen RunMode, der vom PlatformModeInitializer via StartupScript-Konstanten gesetzt wurde
        string currentRunMode = PlayerPrefs.GetString(StartupScript.RunModePlayerPrefKey);
        Debug.Log($"SpectatorManager: Aktueller RunMode aus PlayerPrefs: '{currentRunMode}'");

        if (currentRunMode == StartupScript.SpectatorModeValue)
        {
            Debug.Log("SpectatorManager: Aktiv im Spectator-Modus.");
            InitializeSpectatorMode();
        }
        else
        {
            Debug.Log("SpectatorManager: Nicht im Spectator-Modus, deaktiviere dieses Skript/GameObject.");
            // Stelle sicher, dass die Spectator-Kamera aus ist, falls sie versehentlich aktiv war
            if (spectatorCameraObject != null)
            {
                spectatorCameraObject.SetActive(false);
            }
            // Deaktiviere dieses GameObject, da es im VR-Modus nicht benötigt wird
            gameObject.SetActive(false);
        }
    }

    void InitializeSpectatorMode()
    {
        Debug.Log("SpectatorManager: InitializeSpectatorMode() aufgerufen.");
        // Aktiviere die Spectator-Kamera
        if (spectatorCameraObject != null)
        {
            spectatorCameraObject.SetActive(true);
            Debug.Log("SpectatorManager: Spectator-Kamera aktiviert.");
        }
        else
        {
            Debug.LogError("SpectatorManager: SpectatorCameraObject ist nicht im Inspector zugewiesen!");
        }

        // Setze das PlayerPrefab im NetworkManager auf null, damit kein Spielerobjekt für den Spectator gespawnt wird
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;
            Debug.Log("SpectatorManager: PlayerPrefab für Spectator im NetworkManager auf null gesetzt.");
        }
        else
        {
            Debug.LogError("SpectatorManager: NetworkManager oder NetworkConfig nicht gefunden, um PlayerPrefab zu setzen!");
        }

        // Starte den automatischen LAN-Discovery und Join-Prozess
        if (joinDiscoveryObject != null && lanConnector != null)
        {
            // Nur starten, wenn nicht schon verbunden oder Server (sollte für reinen Client nicht der Fall sein, aber sicher ist sicher)
            if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer))
            {
                joinDiscoveryObject.SetActive(true); // Aktiviere das Objekt, das LanDiscoveryClient startet
                InvokeRepeating(nameof(AttemptToJoinDiscoveredHost), 2.0f, 2.0f); // Versuche alle 2 Sek. zu verbinden
                isActiveAndAttemptingJoin = true;
                Debug.Log("SpectatorManager: Automatischer Join als Spectator gestartet (InvokeRepeating).");
            }
            else {
                Debug.Log("SpectatorManager: Netzwerk ist bereits aktiv oder NetworkManager nicht initialisiert, starte keinen Join-Versuch via InvokeRepeating.");
            }
        }
        else
        {
            Debug.LogError("SpectatorManager: joinDiscoveryObject oder lanConnector nicht korrekt initialisiert. Automatischer Spectator-Join kann nicht gestartet werden!");
        }
    }

    void AttemptToJoinDiscoveredHost()
    {
        if (!isActiveAndAttemptingJoin)
        {
            // Debug.Log("SpectatorManager: AttemptToJoinDiscoveredHost() - isActiveAndAttemptingJoin ist false, breche ab.");
            return; // Stoppe, wenn wir nicht mehr suchen sollen (z.B. nach Erfolg oder Fehler)
        }

        // Debug.Log("SpectatorManager: AttemptToJoinDiscoveredHost() prüft auf gefundenen Host...");
        if (lanConnector != null && lanConnector.discovery != null && !string.IsNullOrEmpty(lanConnector.discovery.foundAddress))
        {
            Debug.Log($"SpectatorManager: Host gefunden unter '{lanConnector.discovery.foundAddress}'. Versuche Verbindung...");

            lanConnector.TryConnect(); // Diese Methode sollte NetworkManager.Singleton.StartClient() aufrufen

            CancelInvoke(nameof(AttemptToJoinDiscoveredHost));
            isActiveAndAttemptingJoin = false;
            Debug.Log("SpectatorManager: InvokeRepeating für AttemptToJoinDiscoveredHost gestoppt nach Verbindungsversuch.");
        }
        else
        {
             Debug.Log("SpectatorManager: Noch kein Host über LAN Discovery gefunden. Suche weiter...");
        }
    }

    void OnDestroy()
    {
        Debug.Log("SpectatorManager: OnDestroy() aufgerufen. Stoppe InvokeRepeating.");
        // Stelle sicher, dass InvokeRepeating gestoppt wird, wenn das Objekt zerstört wird
        CancelInvoke(nameof(AttemptToJoinDiscoveredHost));
    }
}