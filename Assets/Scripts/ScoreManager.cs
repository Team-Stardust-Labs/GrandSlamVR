using UnityEngine;
using Unity.Netcode;
using TMPro; // Erforderlich, wenn du TextMeshPro für deine UI verwendest

// Dieses Skript gehört auf ein GameObject, das immer in der Szene ist
// und über eine NetworkObject-Komponente verfügt (z.B. der NetworkManager selbst).
[RequireComponent(typeof(NetworkObject))]
public class ScoreManager : NetworkBehaviour
{
    [Header("Scoreboard UI Referenzen")]
    [Tooltip("Das Text-Element für den Punktestand von Spieler 1")]
    public TMP_Text scoreTextPlayer1; // Weise dies im Inspector zu!
    [Tooltip("Das Text-Element für den Punktestand von Spieler 2")]
    public TMP_Text scoreTextPlayer2; // Weise dies im Inspector zu!

    // NetworkVariables synchronisieren den Punktestand. Nur der Server darf schreiben.
    public NetworkVariable<int> Player1Score = new NetworkVariable<int>(
        value: 0, // Startwert
        readPerm: NetworkVariableReadPermission.Everyone, // Jeder darf lesen
        writePerm: NetworkVariableWritePermission.Server // Nur Server/Host darf schreiben
    );

    public NetworkVariable<int> Player2Score = new NetworkVariable<int>(
        value: 0,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    // Einfaches Singleton für leichten Zugriff (hauptsächlich für den Server später)
    public static ScoreManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Mehrere ScoreManager Instanzen gefunden. Zerstöre diese.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Optional: Wenn der ScoreManager über Szenenwechsel bestehen bleiben soll:
            // DontDestroyOnLoad(gameObject);
        }
    }

    // Wird aufgerufen, wenn das NetworkObject (und damit dieses Skript) im Netzwerk "erscheint"
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Registriere die Methoden, die aufgerufen werden sollen, wenn sich ein Score ändert.
        // Diese werden auf dem Host UND allen Clients ausgeführt.
        Player1Score.OnValueChanged += OnScoreChangedP1;
        Player2Score.OnValueChanged += OnScoreChangedP2;

        // WICHTIG: Die UI sofort beim Spawn aktualisieren,
        // damit auch spät beitretende Clients den aktuellen Stand sehen.
        UpdateScoreUI(scoreTextPlayer1, Player1Score.Value);
        UpdateScoreUI(scoreTextPlayer2, Player2Score.Value);

        Debug.Log("ScoreManager OnNetworkSpawn: Callbacks registriert und UI initialisiert.");
    }

    // Wird aufgerufen, wenn das NetworkObject aus dem Netzwerk entfernt wird
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // WICHTIG: Callbacks deregistrieren, um Speicherlecks und Fehler zu vermeiden.
        Player1Score.OnValueChanged -= OnScoreChangedP1;
        Player2Score.OnValueChanged -= OnScoreChangedP2;

        if (Instance == this)
        {
            Instance = null;
        }
        Debug.Log("ScoreManager OnNetworkDespawn: Callbacks deregistriert.");
    }

    // --- Callback-Methoden (laufen auf allen Clients + Host/Server) ---

    // Diese Methode wird automatisch aufgerufen, wenn sich Player1Score.Value ändert
    private void OnScoreChangedP1(int previousValue, int newValue)
    {
        Debug.Log($"Score P1 geändert: {previousValue} -> {newValue}");
        UpdateScoreUI(scoreTextPlayer1, newValue);
    }

    // Diese Methode wird automatisch aufgerufen, wenn sich Player2Score.Value ändert
    private void OnScoreChangedP2(int previousValue, int newValue)
    {
        Debug.Log($"Score P2 geändert: {previousValue} -> {newValue}");
        UpdateScoreUI(scoreTextPlayer2, newValue);
    }

    // --- UI Aktualisierung ---

    // Aktualisiert das zugewiesene Text-Element
    private void UpdateScoreUI(TMP_Text uiText, int score)
    {
        if (uiText != null)
        {
            uiText.text = score.ToString();
        }
        else
        {
            // Nur eine Warnung, falls die UI nicht zugewiesen ist, damit das Spiel nicht abstürzt
            Debug.LogWarning($"Versuch, Score UI zu aktualisieren, aber Textfeld ist nicht zugewiesen für Score: {score}");
        }
    }


    // --- Methoden zur Score-Änderung (Platzhalter - Werden später vom Server aufgerufen) ---
    // Diese Methoden MÜSSEN später aus einem Kontext aufgerufen werden, der NUR auf dem SERVER/HOST läuft!

    [ContextMenu("DEBUG: Punkt für P1 geben (Nur Editor/Server)")] // Zum Testen im Editor
    public void AwardPointToPlayer1()
    {
        if (!IsServer) return; // Sicherheitscheck: Nur Server darf dies tun
        Player1Score.Value++;
        Debug.Log($"SERVER: Punkt für P1 vergeben. Neuer Stand: {Player1Score.Value}");
    }

    [ContextMenu("DEBUG: Punkt für P2 geben (Nur Editor/Server)")] // Zum Testen im Editor
    public void AwardPointToPlayer2()
    {
        if (!IsServer) return;
        Player2Score.Value++;
        Debug.Log($"SERVER: Punkt für P2 vergeben. Neuer Stand: {Player2Score.Value}");
    }

    [ContextMenu("DEBUG: Scores zurücksetzen (Nur Editor/Server)")] // Zum Testen im Editor
    public void ResetScores()
    {
        if (!IsServer) return;
        Player1Score.Value = 0;
        Player2Score.Value = 0;
        Debug.Log("SERVER: Scores zurückgesetzt.");
    }
}