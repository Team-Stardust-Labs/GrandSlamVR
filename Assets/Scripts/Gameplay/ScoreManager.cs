using UnityEngine;
using Unity.Netcode;
using TMPro; // Erforderlich, wenn du TextMeshPro f�r deine UI verwendest

// Dieses Skript geh�rt auf ein GameObject, das immer in der Szene ist
// und �ber eine NetworkObject-Komponente verf�gt (z.B. der NetworkManager selbst).
[RequireComponent(typeof(NetworkObject))]
public class ScoreManager : NetworkBehaviour
{
    [Header("Scoreboard UI Referenzen")]
    [Tooltip("Das Text-Element f�r den Punktestand von Spieler 1")]
    public TMP_Text[] scoreTextsPlayer1; // Weise dies im Inspector zu!
    [Tooltip("Das Text-Element f�r den Punktestand von Spieler 2")]
    public TMP_Text[] scoreTextsPlayer2; // Weise dies im Inspector zu!

    // NetworkVariables synchronisieren den Punktestand. Nur der Server darf schreiben.
    public NetworkVariable<int> Player1Score = new NetworkVariable<int>(
        value: 0, // Startwert
        readPerm: NetworkVariableReadPermission.Everyone, // Jeder darf lesen
        writePerm: NetworkVariableWritePermission.Owner // Der Owner darf schreiben
    );

    public NetworkVariable<int> Player2Score = new NetworkVariable<int>(
        value: 0,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Owner
    );

    // Einfaches Singleton für leichten Zugriff (haupts�chlich f�r den Server sp�ter)
    public static ScoreManager Singleton { get; private set; }

    void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Debug.LogWarning("Mehrere ScoreManager Instanzen gefunden. Zerst�re diese.");
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
            // Optional: Wenn der ScoreManager �ber Szenenwechsel bestehen bleiben soll:
            // DontDestroyOnLoad(gameObject);
        }
    }

    // Wird aufgerufen, wenn das NetworkObject (und damit dieses Skript) im Netzwerk "erscheint"
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Registriere die Methoden, die aufgerufen werden sollen, wenn sich ein Score �ndert.
        // Diese werden auf dem Host UND allen Clients ausgef�hrt.
        Player1Score.OnValueChanged += OnScoreChangedP1;
        Player2Score.OnValueChanged += OnScoreChangedP2;

        // WICHTIG: Die UI sofort beim Spawn aktualisieren,
        // damit auch sp�t beitretende Clients den aktuellen Stand sehen.
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

        if (Singleton == this)
        {
            Singleton = null;
        }
        Debug.Log("ScoreManager OnNetworkDespawn: Callbacks deregistriert.");
    }

    // --- Callback-Methoden (laufen auf allen Clients + Host/Server) ---

    // Diese Methode wird automatisch aufgerufen, wenn sich Player1Score.Value �ndert
    private void OnScoreChangedP1(int previousValue, int newValue)
    {
        Debug.Log($"Score P1 ge�ndert: {previousValue} -> {newValue}");
        UpdateScoreUI(scoreTextsPlayer1, newValue);
    }

    // Diese Methode wird automatisch aufgerufen, wenn sich Player2Score.Value �ndert
    private void OnScoreChangedP2(int previousValue, int newValue)
    {
        Debug.Log($"Score P2 ge�ndert: {previousValue} -> {newValue}");
        UpdateScoreUI(scoreTextsPlayer2, newValue);
    }

    // --- UI Aktualisierung ---

    // Aktualisiert das zugewiesene Text-Element
    private void UpdateScoreUI(TMP_Text[] uiTexts, int score)
    {
        foreach (TMP_Text uiText in uiTexts)
        {
            if (uiText != null)
            {
                uiText.text = score.ToString();
            }
            else
            {
                // Nur eine Warnung, falls die UI nicht zugewiesen ist, damit das Spiel nicht abst�rzt
                Debug.LogWarning($"Versuch, Score UI zu aktualisieren, aber Textfeld ist nicht zugewiesen f�r Score: {score}");
            }
        }
    }


    // --- Methoden zur Score-�nderung (Platzhalter - Werden sp�ter vom Server aufgerufen) ---
    // Diese Methoden M�SSEN sp�ter aus einem Kontext aufgerufen werden, der NUR auf dem SERVER/HOST l�uft!

    public void AwardPointToPlayer1()
    {

        // Punkt f�r Spieler 1 hinzuf�gen
        Player1Score.Value++;

        // Log, um den aktuellen Stand auf dem Server anzuzeigen (kann beibehalten werden)
        CustomDebugLog.Singleton.LogNetworkManager($"SERVER: Punkt f�r P1 vergeben. Neuer Stand: {Player1Score.Value}");

        // �berpr�fen, ob Spieler 1 die erforderlichen 5 Punkte erreicht hat
        if (Player1Score.Value >= 5) // Oft pr�ft man >=, falls durch schnelle Ereignisse der Wert 5 �bersprungen wird
        {
            // Punkte-Limit erreicht (oder �berschritten), Score zur�cksetzen
            ResetScores();
            // Optionale Log-Meldung, wenn der Score zur�ckgesetzt wird
            CustomDebugLog.Singleton.LogNetworkManager("SERVER: Spieler 1 hat 5 Punkte erreicht. Score wird zur�ckgesetzt.");
        }

    }
    public void AwardPointToPlayer2()
    {

        // Punkt f�r Spieler 1 hinzuf�gen
        Player2Score.Value++;

        // Log, um den aktuellen Stand auf dem Server anzuzeigen (kann beibehalten werden)
        CustomDebugLog.Singleton.LogNetworkManager($"SERVER: Punkt f�r P2 vergeben. Neuer Stand: {Player2Score.Value}");

        // �berpr�fen, ob Spieler 1 die erforderlichen 5 Punkte erreicht hat
        if (Player2Score.Value >= 5) // Oft pr�ft man >=, falls durch schnelle Ereignisse der Wert 5 �bersprungen wird
        {
            // Punkte-Limit erreicht (oder �berschritten), Score zur�cksetzen
            ResetScores();
            // Optionale Log-Meldung, wenn der Score zur�ckgesetzt wird
            CustomDebugLog.Singleton.LogNetworkManager("SERVER: Spieler 1 hat 5 Punkte erreicht. Score wird zur�ckgesetzt.");
        }


    }
    public void ResetScores()
    {
        Player1Score.Value = 0;
        Player2Score.Value = 0;
        CustomDebugLog.Singleton.LogNetworkManager("SERVER: Scores zur�ckgesetzt.");
    }
}