using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer;

// Ensures that Rigidbody and NetworkPhysicsInteractable components are attached
[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(NetworkPhysicsInteractable))]
public class BallScoring : MonoBehaviour
{
    [SerializeField] private int maxBounces = 1; // Maximum allowed bounces before scoring
    private int bounces = 0; // Current bounce count

    [SerializeField] private Transform ballSpawnPlayer1; // Spawn point for player 1
    [SerializeField] private Transform ballSpawnPlayer2; // Spawn point for player 2
    private Transform currentBallSpawn; // Used to set the next spawn point for the ball

    private Rigidbody m_rigidbody; // Reference to the Rigidbody component
    public NetworkPhysicsInteractable m_networkPhysicsInteractable; // Reference to the network physics component

    [SerializeField] private Material defaultBallMaterial; // Reference to the default ball material (assign in Inspector)
    private Renderer m_renderer; // Renderer component for material changes

    public CameraSwitching spectator; // Reference to spectator camera logic (optional)

    // Initialization of components and material assignment
    void Start()
    {
        currentBallSpawn = new GameObject("DefaultSpawn").transform;
        m_rigidbody = GetComponent<Rigidbody>();
        m_networkPhysicsInteractable = GetComponent<NetworkPhysicsInteractable>();
        m_renderer = GetComponent<Renderer>();

        if (m_renderer == null)
        {
            Debug.LogError("BallScoring: Renderer component not found on GameObject.", this.gameObject);
            return;
        }

        // Fallback if no material is assigned in the Inspector
        if (defaultBallMaterial == null)
        {
            Debug.LogWarning("BallScoring: defaultBallMaterial not assigned in Inspector. Using current renderer's sharedMaterial as default. Please assign it in the Inspector for robustness.", this.gameObject);
            defaultBallMaterial = m_renderer.sharedMaterial;
            if (defaultBallMaterial == null)
            {
                Debug.LogError("BallScoring: Renderer has no sharedMaterial to use as default, and defaultBallMaterial was not assigned.", this.gameObject);
                return;
            }
        }
        else
        {
            // Use the assigned defaultBallMaterial
            if (m_renderer.sharedMaterial != defaultBallMaterial)
            {
                m_renderer.sharedMaterial = defaultBallMaterial;
            }
        }
    }

    // Resets the bounce count (called on respawn or when the ball is grabbed)
    public void ResetBounces()
    {
        bounces = 0;
    }

    // Resets the ball color (called on respawn or when the ball is grabbed)
    public void ResetColor()
    {
        if (defaultBallMaterial == null)
        {
            Debug.LogError("BallScoring: defaultBallMaterial is not set. Cannot reset color. Please assign it in the Inspector or ensure it's picked up in Start().", this.gameObject);
            return;
        }

        // Ensure the default material is set
        if (m_renderer.sharedMaterial != defaultBallMaterial)
        {
            m_renderer.sharedMaterial = defaultBallMaterial;
        }

        // Set color and emission to default values
        defaultBallMaterial.SetColor(Shader.PropertyToID("_Color"), new Color(0.816f, 1.0f, 0.0f));
        defaultBallMaterial.SetColor(Shader.PropertyToID("_EmissionColor"), new Color(0.3066064f, 0.6588235f, 0.2941176f));
    }

    // Handles score updates and determines which player gets the point and where the ball respawns
    // penaltyForLastPlayerThrown: If true, the last player who threw the ball gets a penalty
    private void UpdateScore(bool penaltyForLastPlayerThrown = false)
    {
        // Triggers when penalty is to be applied
        if (penaltyForLastPlayerThrown && m_networkPhysicsInteractable.isThrown == true)
        {
            // Give point to the opponent, respawn ball on their side
            if (m_networkPhysicsInteractable.lastThrownPlayerColor == AssignPlayerColor.PlayerColor.Blue)
            {
                currentBallSpawn = ballSpawnPlayer2; // Spawn on red side
                ScoreManager.Singleton.PointToPlayer2Request(); // ScoreManager handles Scoring
                return;
            }
            else if (m_networkPhysicsInteractable.lastThrownPlayerColor == AssignPlayerColor.PlayerColor.Red)
            {
                currentBallSpawn = ballSpawnPlayer1; // Spawn on blue side
                ScoreManager.Singleton.PointToPlayer1Request(); // ScoreManager handles Scoring
                return;
            }
        }

        // Normal scoring based on ball position
        if (transform.position.x > 0.0f)
        {
            // Point for player 1, ball to player 2
            currentBallSpawn = ballSpawnPlayer2;
            ScoreManager.Singleton.PointToPlayer1Request(); // ScoreManager handles Scoring
        }
        else
        {
            // Point for player 2, ball to player 1
            currentBallSpawn = ballSpawnPlayer1;
            ScoreManager.Singleton.PointToPlayer2Request(); // ScoreManager handles Scoring
        }
    }

    // Respawn logic when the ball is reset via button
    public void RespawnButtonCode()
    {
        currentBallSpawn.position = new Vector3(-70.0f, 5.0f, 0);

        // Only move the object if we are the owner
        if (m_networkPhysicsInteractable.IsOwner)
        {
            m_networkPhysicsInteractable.Ungrab();
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.MovePosition(currentBallSpawn.position);
            m_networkPhysicsInteractable.isThrown = false;
            m_networkPhysicsInteractable.deactivateTrailsRpc();
        }
    }

    // Respawn logic after scoring or error
    private void RespawnBall()
    {
        // Resets the ball attributes
        ResetBounces();
        ResetColor();

        // Fallback spawn points if not set
        if (ballSpawnPlayer2 == null)
        {
            currentBallSpawn.position = ballSpawnPlayer1.position; // Default to player 1 spawn if player 2 spawn is not set
        }

        if (ballSpawnPlayer1 == null)
        {
            currentBallSpawn.position = new Vector3(0, 5.0f, 0); // Default to center if player 1 spawn is not set
        }

        if (currentBallSpawn == null)
        {
            currentBallSpawn.position = ballSpawnPlayer1.position; // Default to player 1 spawn if currentBallSpawn is not set
        }

        // Only move the object if we are the owner
        if (m_networkPhysicsInteractable.IsOwner)
        {
            m_networkPhysicsInteractable.Ungrab();
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.MovePosition(currentBallSpawn.position);
            m_networkPhysicsInteractable.isThrown = false;
            m_networkPhysicsInteractable.deactivateTrailsRpc();
        }
    }

    // Handles collision logic: scoring, bounce counting, color update
    private void OnCollisionEnter(Collision collision)
    {
        // Do nothing if the ball is in hand or we are not the owner
        if (m_rigidbody.isKinematic || !m_networkPhysicsInteractable.IsOwner)
        {
            return;
        }

        // Regular bouncing and scoring
        if (collision.gameObject.CompareTag("Court") && m_networkPhysicsInteractable.isThrown == true)
        {
            // Bounces of Player Blue only count on the Red Side
            if (m_networkPhysicsInteractable.lastThrownPlayerColor == AssignPlayerColor.PlayerColor.Blue && transform.position.x > 0.0f)
            {
                bounces++;
            }
            // Bounces of Player Red only count on the Blue Side
            else if (m_networkPhysicsInteractable.lastThrownPlayerColor == AssignPlayerColor.PlayerColor.Red && transform.position.x < 0.0f)
            {
                bounces++;
            }

            // If bounces exceed max, update score and respawn
            if (bounces > maxBounces)
            {
                UpdateScore();
                RespawnBall();
                return;
            }

            // Update ball color based on bounce count
            if (m_renderer != null && defaultBallMaterial != null)
            {
                if (m_renderer.sharedMaterial != defaultBallMaterial)
                {
                    m_renderer.sharedMaterial = defaultBallMaterial;
                }
                float colorValue = Mathf.Max((float)bounces, 1.0f) / (float)maxBounces;
                defaultBallMaterial.SetColor(Shader.PropertyToID("_Color"), new Color(1.0f, 1.0f - colorValue * 1.5f, 1.0f - colorValue * 1.5f));
            }
        }

        if (collision.gameObject.CompareTag("Respawn"))
        {
            UpdateScore(true); // Penalty Flag is set
            RespawnBall();
            return;
        }

        // Penalty if colliding with BoundingBox (out of Bounds)
        if (collision.gameObject.CompareTag("BoundingBox"))
        {
            UpdateScore(true); // Penalty flag is set
            RespawnBall();
            return;
        }
    }
}
