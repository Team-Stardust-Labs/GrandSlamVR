using UnityEngine;
using Unity.Netcode;
using TMPro; // Erforderlich, wenn du TextMeshPro f�r deine UI verwendest

/*
    ScoreManager Overview

    - Tracks and manages the score for both players.
    - Ends the game once a player reaches the winning score.
    - Uses callbacks to keep the UI in sync with the score.
    - Singleton pattern allows global access to the ScoreManager.

    Networking:
    - Uses NetworkVariables to sync score across server and clients.
    - Only the server is allowed to modify scores.
    - All clients can request to award points using PointToPlayer1Request() and PointToPlayer2Request().

    Spectator Support:
    - Notifies the spectator system about important game events (e.g., scoring, game end).
*/


// This script should be attached to a persistent GameObject with a NetworkObject component,
// such as the object containing the NetworkManager.
[RequireComponent(typeof(NetworkObject))]
public class ScoreManager : NetworkBehaviour
{
    [Header("Scoreboard UI Referenzen")]
    [Tooltip("Das Text-Element f�r den Punktestand von Spieler 1")]
    public TMP_Text[] scoreTextsPlayer1; // Drag all TextMeshPro UI Text elements for Player 1 here in the Inspector that have to be updated
    [Tooltip("Das Text-Element f�r den Punktestand von Spieler 2")]
    public TMP_Text[] scoreTextsPlayer2; // Drag all TextMeshPro UI Text elements for Player 2 here in the Inspector that have to be updated

    // Various sounds for scoring and game end
    public AudioSource m_scoreSound;
    public AudioSource m_scorelostSound;
    public AudioSource m_winGameSound;
    public AudioSource m_loseGameSound;

    // References to the spectator and announcer system for playing the specator sound clips and switching the camera to the scoreboard
    public CameraSwitching spectator;
    public AnnouncerMananger announcer;

    // Variable to track if the game is currently running
    private static bool gameRunning;

    // NetworkVariables to store the scores for both players.
    public NetworkVariable<int> Player1Score = new NetworkVariable<int>(
        value: 0, // Strt at 0
        readPerm: NetworkVariableReadPermission.Everyone, // Everyone can read the score
        writePerm: NetworkVariableWritePermission.Server // Only the server can write to the score
    );

    // Same as Player1Score, but for Player 2
    public NetworkVariable<int> Player2Score = new NetworkVariable<int>(
        value: 0,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    // Simple Singleton instance of ScoreManager for global access
    public static ScoreManager Singleton { get; private set; }

    void Awake()
    {
        // Singleton pattern to ensure only one instance of ScoreManager exists
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
        }
        gameRunning = true; // Game is now running, set gameRunning to true
    }

    // Returns the current gameRunning state
    public static bool isGameFinished()
    {
        return !gameRunning;
    }

    // Returns true if Player 1 is the winner (i.e., has more points than Player 2)
    public bool isBlueWinner()
    {
        return Player1Score.Value > Player2Score.Value;
    }

    // Called when the NetworkObject is spawned in the network.
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Register callbacks to respond to score changes
        // Run these methods on the Host AND all Clients.
        Player1Score.OnValueChanged += OnScoreChangedP1;
        Player2Score.OnValueChanged += OnScoreChangedP2;

        // Immediately update the UI for late joiners
        UpdateScoreUI(scoreTextsPlayer1, Player1Score.Value);
        UpdateScoreUI(scoreTextsPlayer2, Player2Score.Value);
    }

    // Called when the NetworkObject is despawned from the network.
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Clean up to prevent memory leaks or duplicate callbacks
        Player1Score.OnValueChanged -= OnScoreChangedP1;
        Player2Score.OnValueChanged -= OnScoreChangedP2;

        if (Singleton == this)
        {
            Singleton = null;
        }
    }

    // --- Score Change Callbacks ---

    // Called when Player1Score.Value changes
    private void OnScoreChangedP1(int previousValue, int newValue)
    {
        UpdateScoreUI(scoreTextsPlayer1, newValue); // Update the UI for Player 1's score
    }

    // Called when Player2Score.Value changes
    private void OnScoreChangedP2(int previousValue, int newValue)
    {
        UpdateScoreUI(scoreTextsPlayer2, newValue); // Update the UI for Player 2's score
    }

    // --- UI Updates ---

    // Update UI text elements within the set Array with the current score
    private void UpdateScoreUI(TMP_Text[] uiTexts, int score)
    {
        foreach (TMP_Text uiText in uiTexts) // either Array of Player1Score UI or Player2Score UI Texts
        {
            if (uiText != null)
            {
                uiText.text = score.ToString(); // Update the text to show the current score
            }
        }

    }

    // --- Networked Score Management ---

    // These methods are called by the clients to request a point for a player.
    [Rpc(SendTo.Server)]
    private void ClientPointToPlayer1Rpc() {
        ServerPointToPlayer1();
    }

    [Rpc(SendTo.Server)]
    private void ClientPointToPlayer2Rpc()
    {
        ServerPointToPlayer2();
    }

    // These methods are called by the server to award points to players.
    private void ServerPointToPlayer1()
    {

        if (!IsServer) // Only for the server
        { 
            return;
        }

        // Point to Player 1
        Player1Score.Value++;

        // Log the score change on the server
        CustomDebugLog.Singleton.LogNetworkManager($"SERVER: Punkt an P1 vergeben. Neuer Stand: {Player1Score.Value}");

        // Check for winning condition for Player 1
        if (Player1Score.Value >= 7)
        {
            stopGameRpc(); // Stop the game for everyone
            CustomDebugLog.Singleton.LogNetworkManager("SERVER: Spieler 1 hat 7 Punkte erreicht. Score wird zur�ckgesetzt."); // Debug log of the Victory

            // Queue win sound with the parameter of the winner
            playWinLoseSoundRpc(AssignPlayerColor.PlayerColor.Blue);
        }
        else
        {
            // Queue score sound with the parameter of the player who scored
            playScoreSoundRpc(AssignPlayerColor.PlayerColor.Blue);
        }
    }

    private void ServerPointToPlayer2()
    {

        if (!IsServer) // Only for the server
        {
            return;
        }

        // Point to Player 2
        Player2Score.Value++;

        // Log the score change on the server
        CustomDebugLog.Singleton.LogNetworkManager($"SERVER: Punkt f�r P2 vergeben. Neuer Stand: {Player2Score.Value}");

        // Check for winning condition for Player 2
        if (Player2Score.Value >= 7) 
        {
            stopGameRpc(); // Stop the game for everyone
            CustomDebugLog.Singleton.LogNetworkManager("SERVER: Spieler 1 hat 7 Punkte erreicht. Score wird zur�ckgesetzt."); // Debug log of the Victory

            // Queue win sound with the parameter of the winner
            playWinLoseSoundRpc(AssignPlayerColor.PlayerColor.Red);
        }
        else
        {
            // Queue score sound with the parameter of the player who scored
            playScoreSoundRpc(AssignPlayerColor.PlayerColor.Red);
        }
    }

    // Method to play the score sound for the respective player
    // Param is the PlayerColor of the player who scored
    [Rpc(SendTo.Everyone)]
    private void playScoreSoundRpc(AssignPlayerColor.PlayerColor playerColor)
    {
        spectator.OnScoreChanged(); // Notify the spectator system that a score has changed to switch camera to the scoreboard
        if (playerColor == AssignPlayerColor.getPlayerColor())
        {
            m_scoreSound.Play(); // Play the score sound for the player who scored
        }
        else
        {
            m_scorelostSound.Play(); // Play the score lost sound for the player who did not score
        }

        // Spectator plays seperate announcer sound for matchpoints
        if (Player1Score.Value == 5) // equals 6 because this is called before adding to score (matchpoint to 7 is 6)
        {
            // Play either Matchpoint sound for Player 1 or Player 2
            if (playerColor == AssignPlayerColor.PlayerColor.Blue)
            {
                announcer.PlayMatchpointBlue();
                return;
            }
        }

        if (Player2Score.Value == 5)// equals 6 because this is called before adding to score (matchpoint to 7 is 6)
        {
            if (playerColor == AssignPlayerColor.PlayerColor.Red)
            {
                announcer.PlayMatchpointRed();
                return;
            }
        }

        // Otherwise play the normal announcer sound for the respective player
        if (playerColor == AssignPlayerColor.PlayerColor.Blue)
        {
            announcer.PlayPointBlue();
            return;
        }

        if (playerColor == AssignPlayerColor.PlayerColor.Red)
        {
            announcer.PlayPointRed();
            return;
        }
    }

    // Method to stop the game for everyone
    [Rpc(SendTo.Everyone)]
    private void stopGameRpc()
    {
        gameRunning = false;
    }

    // Method to play the win/lose sound for the respective player
    // Param is the Winner's PlayerColor
    [Rpc(SendTo.Everyone)]
    private void playWinLoseSoundRpc(AssignPlayerColor.PlayerColor playerColor)
    {
        if (playerColor == AssignPlayerColor.getPlayerColor())
        {
            m_winGameSound.Play(); // Play the win sound for the winning player
        }
        else
        {
            m_loseGameSound.Play(); // Play the lose sound for the losing player
        }

        // Spectator plays announcer sound with either Blue or Red
        if (playerColor == AssignPlayerColor.PlayerColor.Blue)
        {
            announcer.PlayWinBlue(); // Spectator plays announcer sound with Blue
        }

        if (playerColor == AssignPlayerColor.PlayerColor.Red)
        {
            announcer.PlayWinRed(); // Spectator plays announcer sound with Red
        }
    }


    private void ServerResetScores()
    {
        if (!IsServer) {
            return;
        }

        Player1Score.Value = 0;
        Player2Score.Value = 0;
        CustomDebugLog.Singleton.LogNetworkManager("SERVER: Scores zur�ckgesetzt.");
    }


    // call this function to award a point to player 1
    public void PointToPlayer1Request() {
        if (IsServer) {
            ServerPointToPlayer1();
            return;
        }

        ClientPointToPlayer1Rpc();
    }

    // call this function to award a point to player 2
    public void PointToPlayer2Request() {
        if (IsServer) {
            ServerPointToPlayer2();
            return;
        }

        ClientPointToPlayer2Rpc();
    }
}