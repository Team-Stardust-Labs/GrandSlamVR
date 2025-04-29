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
    public TMP_Text[] scoreTextsPlayer1; // Weise dies im Inspector zu!
    [Tooltip("Das Text-Element für den Punktestand von Spieler 2")]
    public TMP_Text[] scoreTextsPlayer2; // Weise dies im Inspector zu!

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
        UpdateScoreUI(scoreTextsPlayer1, Player1Score.Value);
        UpdateScoreUI(scoreTextsPlayer2, Player2Score.Value);

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
        UpdateScoreUI(scoreTextsPlayer1, newValue);
    }

    // Diese Methode wird automatisch aufgerufen, wenn sich Player2Score.Value ändert
    private void OnScoreChangedP2(int previousValue, int newValue)
    {
        Debug.Log($"Score P2 geändert: {previousValue} -> {newValue}");
        UpdateScoreUI(scoreTextsPlayer2, newValue);
    }

    // --- UI Aktualisierung ---

    // Aktualisiert das zugewiesene Text-Element
    private void UpdateScoreUI(TMP_Text[] uiTexts, int score)
    {
        foreach (TMP_Text uiText in uiTexts)
        { if (uiText != null)
            {
                uiText.text = score.ToString();
            }
            else
            {
                // Nur eine Warnung, falls die UI nicht zugewiesen ist, damit das Spiel nicht abstürzt
                Debug.LogWarning($"Versuch, Score UI zu aktualisieren, aber Textfeld ist nicht zugewiesen für Score: {score}");
            }
        } 
    }


    // --- Methoden zur Score-Änderung (Platzhalter - Werden später vom Server aufgerufen) ---
    // Diese Methoden MÜSSEN später aus einem Kontext aufgerufen werden, der NUR auf dem SERVER/HOST läuft!

    [ContextMenu("DEBUG: Punkt für P1 geben (Nur Editor/Server)")] // Zum Testen im Editor
    public void AwardPointToPlayer1()
    {
        // Sicherheitscheck: Nur Server darf dies tun.
        // Da Player1Score wahrscheinlich eine NetworkVariable ist, muss die Änderung serverseitig erfolgen.
        if (!IsServer)
        {
            return;
        }

        // Punkt für Spieler 1 hinzufügen
        Player1Score.Value++;

        // Log, um den aktuellen Stand auf dem Server anzuzeigen (kann beibehalten werden)
        Debug.Log($"SERVER: Punkt für P1 vergeben. Neuer Stand: {Player1Score.Value}");

        // --- NEUE LOGIK STARTET HIER ---

        // Überprüfen, ob Spieler 1 die erforderlichen 5 Punkte erreicht hat
        if (Player1Score.Value >= 5) // Oft prüft man >=, falls durch schnelle Ereignisse der Wert 5 übersprungen wird
        {
            // Punkte-Limit erreicht (oder überschritten), Score zurücksetzen
           ResetScores();
            // Optionale Log-Meldung, wenn der Score zurückgesetzt wird
            Debug.Log("SERVER: Spieler 1 hat 5 Punkte erreicht. Score wird zurückgesetzt.");
        }

        // --- NEUE LOGIK ENDET HIER ---

    }
    public void AwardPointToPlayer2()
    {
        // Sicherheitscheck: Nur Server darf dies tun.
        // Da Player2Score wahrscheinlich eine NetworkVariable ist, muss die Änderung serverseitig erfolgen.
        if (!IsServer)
        {
            return;
        }

        // Punkt für Spieler 1 hinzufügen
        Player2Score.Value++;

        // Log, um den aktuellen Stand auf dem Server anzuzeigen (kann beibehalten werden)
        Debug.Log($"SERVER: Punkt für P2 vergeben. Neuer Stand: {Player2Score.Value}");

        // --- NEUE LOGIK STARTET HIER ---

        // Überprüfen, ob Spieler 1 die erforderlichen 5 Punkte erreicht hat
        if (Player2Score.Value >= 5) // Oft prüft man >=, falls durch schnelle Ereignisse der Wert 5 übersprungen wird
        {
            // Punkte-Limit erreicht (oder überschritten), Score zurücksetzen
            ResetScores();
            // Optionale Log-Meldung, wenn der Score zurückgesetzt wird
            Debug.Log("SERVER: Spieler 1 hat 5 Punkte erreicht. Score wird zurückgesetzt.");
        }

        // --- NEUE LOGIK ENDET HIER ---

    }
    public void ResetScores()
    {
        if (!IsServer) return;
        Player1Score.Value = 0;
        Player2Score.Value = 0;
        Debug.Log("SERVER: Scores zurückgesetzt.");
    }
}