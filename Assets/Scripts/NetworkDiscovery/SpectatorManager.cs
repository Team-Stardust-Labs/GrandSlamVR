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
        // Hole die Referenz zum LanConnector-Skript auf dem zugewiesenen joinDiscoveryObject
        if (joinDiscoveryObject != null)
        {
            lanConnector = joinDiscoveryObject.GetComponent<ConnectToDiscoveredHost>();
            if (lanConnector == null)
            {
                Debug.LogError("SpectatorManager: ConnectToDiscoveredHost Skript nicht auf joinDiscoveryObject gefunden!");
            }
        }
        else
        {
            Debug.LogWarning("SpectatorManager: joinDiscoveryObject ist nicht im Inspector zugewiesen!");
        }
    }

    void Start()
    {
        // Lese den aktuellen RunMode, der vom PlatformModeInitializer via StartupScript-Konstanten gesetzt wurde
        string currentRunMode = PlayerPrefs.GetString(StartupScript.RunModePlayerPrefKey);
        Debug.Log($"SpectatorManager: Aktueller RunMode aus PlayerPrefs: {currentRunMode}");

        if (currentRunMode == StartupScript.SpectatorModeValue)
        {
            // CustomDebugLog.Singleton.Log("SpectatorManager: Aktiv im Spectator-Modus.");
            InitializeSpectatorMode();
        }
        else
        {
            // CustomDebugLog.Singleton.Log("SpectatorManager: Nicht im Spectator-Modus, deaktiviere dieses Skript/GameObject.");
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
        // Aktiviere die Spectator-Kamera
        if (spectatorCameraObject != null)
        {
            spectatorCameraObject.SetActive(true);
            // CustomDebugLog.Singleton.Log("SpectatorManager: Spectator-Kamera aktiviert.");
        }
        else
        {
            Debug.LogError("SpectatorManager: SpectatorCameraObject ist nicht im Inspector zugewiesen!");
        }

        // Setze das PlayerPrefab im NetworkManager auf null, damit kein Spielerobjekt für den Spectator gespawnt wird
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkConfig != null)
        {
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;
            // CustomDebugLog.Singleton.Log("SpectatorManager: PlayerPrefab für Spectator im NetworkManager auf null gesetzt.");
        }
        else
        {
            Debug.LogError("SpectatorManager: NetworkManager oder NetworkConfig nicht gefunden, um PlayerPrefab zu setzen!");
        }

        // Starte den automatischen LAN-Discovery und Join-Prozess
        if (joinDiscoveryObject != null && lanConnector != null)
        {
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) // Nur starten, wenn nicht schon verbunden
            {
                joinDiscoveryObject.SetActive(true); // Aktiviere das Objekt, das LanDiscoveryClient startet
                InvokeRepeating(nameof(AttemptToJoinDiscoveredHost), 2.0f, 2.0f); // Versuche alle 2 Sek. zu verbinden
                isActiveAndAttemptingJoin = true;
                // CustomDebugLog.Singleton.Log("SpectatorManager: Automatischer Join als Spectator gestartet (InvokeRepeating).");
            }
            // else CustomDebugLog.Singleton.Log("SpectatorManager: Netzwerk ist bereits aktiv, starte keinen Join-Versuch.");
        }
        else
        {
            Debug.LogError("SpectatorManager: joinDiscoveryObject oder lanConnector nicht korrekt initialisiert. Automatischer Spectator-Join kann nicht gestartet werden!");
        }
    }

    void AttemptToJoinDiscoveredHost()
    {
        if (!isActiveAndAttemptingJoin) return; // Stoppe, wenn wir nicht mehr suchen sollen (z.B. nach Erfolg)

        // Prüfe, ob der LanConnector (ConnectToDiscoveredHost) eine Adresse gefunden hat
        if (lanConnector != null && !string.IsNullOrEmpty(lanConnector.discovery.foundAddress))
        {
            // CustomDebugLog.Singleton.Log($"SpectatorManager: Host gefunden unter {lanConnector.discovery.foundAddress}. Versuche Verbindung...");

            // Rufe die TryConnect-Methode auf dem LanConnector auf.
            // Diese Methode sollte NetworkManager.Singleton.StartClient() mit der gefundenen Adresse aufrufen.
            lanConnector.TryConnect();

            // Stoppe weitere Join-Versuche, nachdem der erste Versuch unternommen wurde.
            // ConnectToDiscoveredHost.TryConnect() gibt einen bool zurück, den wir hier nicht direkt abfragen,
            // aber wir gehen davon aus, dass der Versuch gestartet wird.
            // Der NetworkManager wird dann Events für erfolgreiche/fehlgeschlagene Verbindung auslösen.
            CancelInvoke(nameof(AttemptToJoinDiscoveredHost));
            isActiveAndAttemptingJoin = false; // Markiere, dass wir den Join-Prozess abgeschlossen haben (Versuch gestartet)
            // CustomDebugLog.Singleton.Log("SpectatorManager: InvokeRepeating für AttemptToJoinDiscoveredHost gestoppt nach Verbindungsversuch.");
        }
        // else CustomDebugLog.Singleton.Log("SpectatorManager: Noch kein Host über LAN Discovery gefunden. Suche weiter...");
    }

    void OnDestroy()
    {
        // Stelle sicher, dass InvokeRepeating gestoppt wird, wenn das Objekt zerstört wird
        CancelInvoke(nameof(AttemptToJoinDiscoveredHost));
    }
}