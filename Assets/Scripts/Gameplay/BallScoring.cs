using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(NetworkPhysicsInteractable))]
public class BallScoring : MonoBehaviour
{
    [SerializeField] private int maxBounces = 1; // Max bounces before scoring
    private int bounces = 0;
    [SerializeField] private Transform ballSpawnPlayer1;
    [SerializeField] private Transform ballSpawnPlayer2;
    private Transform currentBallSpawn;

    private Rigidbody m_rigidbody;
    public NetworkPhysicsInteractable m_networkPhysicsInteractable;
    
    // Added: Reference to the default material, assign in Inspector
    [SerializeField] private Material defaultBallMaterial; 
    private Renderer m_renderer;

    public CameraSwitching spectator;

    
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

        if (defaultBallMaterial == null)
        {
            Debug.LogWarning("BallScoring: defaultBallMaterial not assigned in Inspector. Using current renderer's sharedMaterial as default. Please assign it in the Inspector for robustness.", this.gameObject);
            defaultBallMaterial = m_renderer.sharedMaterial;
            if (defaultBallMaterial == null) {
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

    // Bounces reset on respawn or grab
    // gets called by networkphysicsinteractable on select entered
    public void ResetBounces() {
        bounces = 0;
    }   

    // Color resets on respawn or grab
    // gets called by networkphysicsinteractable on select entered
    public void ResetColor() {
        if (defaultBallMaterial == null) {
            Debug.LogError("BallScoring: defaultBallMaterial is not set. Cannot reset color. Please assign it in the Inspector or ensure it's picked up in Start().", this.gameObject);
            return;
        }

        // Ensure default material is set
        if (m_renderer.sharedMaterial != defaultBallMaterial) {
            m_renderer.sharedMaterial = defaultBallMaterial;
        }

        defaultBallMaterial.SetColor(Shader.PropertyToID("_Color"), new Color(0.816f, 1.0f, 0.0f));
        defaultBallMaterial.SetColor(Shader.PropertyToID("_EmissionColor"), new Color(0.3066064f, 0.6588235f, 0.2941176f));
    }

    void FixedUpdate() {
        /* BUGGY
        // Reset ball if ball gets stale / dosent move or is out of bounds
        if (m_rigidbody.linearVelocity.magnitude < 1.0 && m_networkPhysicsInteractable.isThrown == true)
        {
            // Free retry for the same player that had the ball
            RespawnBall();
        }*/
    }

    private void UpdateScore(bool penaltyForLastPlayerThrown = false) { // if penaltyForLastPlayerThrown is true, the player who threw the ball last gets a penalty

        if (penaltyForLastPlayerThrown && m_networkPhysicsInteractable.isThrown == true)
        {

            if (m_networkPhysicsInteractable.lastThrownPlayerColor == AssignPlayerColor.PlayerColor.Blue)
            {
                currentBallSpawn = ballSpawnPlayer2;
                ScoreManager.Singleton.PointToPlayer2Request();
                return;
            }
            else if (m_networkPhysicsInteractable.lastThrownPlayerColor == AssignPlayerColor.PlayerColor.Red)
            {
                currentBallSpawn = ballSpawnPlayer1;
                ScoreManager.Singleton.PointToPlayer1Request();
                return;
            }
        }

        // Update score 
        if (transform.position.x > 0.0f)
        {
            // score for player1, but give ball to player2
            currentBallSpawn = ballSpawnPlayer2;

            ScoreManager.Singleton.PointToPlayer1Request();
        }
        else
        {
            // score for player2, but give ball to player1
            currentBallSpawn = ballSpawnPlayer1;

            ScoreManager.Singleton.PointToPlayer2Request();
        }
    }

    public void RespawnButtonCode()
    {
        currentBallSpawn.position = new Vector3(-70.0f, 5.0f, 0);

        // only move the object if we are the owner
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

    private void RespawnBall()
    {

        ResetBounces();
        ResetColor();

        // Fallback spawnpoints
        if (ballSpawnPlayer2 == null)
        {
            currentBallSpawn.position = ballSpawnPlayer1.position;
        }

        if (ballSpawnPlayer1 == null)
        {
            currentBallSpawn.position = new Vector3(0, 5.0f, 0);
        }

        if (currentBallSpawn == null)
        {
            currentBallSpawn.position = ballSpawnPlayer1.position;
        }

        // only move the object if we are the owner
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

    private void OnCollisionEnter(Collision collision)
    {

        // If ball is in hand of player or we arent the owner, do nothing
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

                if (bounces > maxBounces)
            {
                UpdateScore();
                RespawnBall();
                return;
            }

            // Update color
            if (m_renderer != null && defaultBallMaterial != null)
            {
                if (m_renderer.sharedMaterial != defaultBallMaterial) {
                    m_renderer.sharedMaterial = defaultBallMaterial;
                }
                float colorValue = (float)bounces / (float)maxBounces;
                defaultBallMaterial.SetColor(Shader.PropertyToID("_Color"), new Color(1.0f, 1.0f - colorValue * 1.5f, 1.0f - colorValue * 1.5f));
            }
        }

        
        if (collision.gameObject.CompareTag("Respawn"))
        {
            UpdateScore(true); // skip scoring
            RespawnBall();
            return;
        }

        
        if (collision.gameObject.CompareTag("BoundingBox"))
        {
            UpdateScore(true); // penalty for last player thrown
            RespawnBall();   
        }
    }

    private bool getIsThrown()
    {
        return m_networkPhysicsInteractable.isThrown;
    }
}
