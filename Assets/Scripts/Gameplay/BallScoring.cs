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

    // Nico: Added NetworkPhysicsInteractable reference to make the isThrown variable non static
    // so that multiple balls could be supported in the future
    private NetworkPhysicsInteractable m_networkPhysicsInteractable;

    
    void Start()
    {
        currentBallSpawn = new GameObject("DefaultSpawn").transform;
        m_rigidbody = GetComponent<Rigidbody>();
        m_networkPhysicsInteractable = GetComponent<NetworkPhysicsInteractable>();
    }

    // Bounces reset on respawn or grab
    // gets called by networkphysicsinteractable on select entered
    public void ResetBounces() {
        bounces = 0;
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

    private void UpdateScore(bool skipScoring = false) {
        // Update score 
        if (transform.position.x > 0.0f)
        {
            // score for player1, but give ball to player2
            currentBallSpawn = ballSpawnPlayer2;

            if (skipScoring) {
                return;
            }
            ScoreManager.Singleton.PointToPlayer1Request();
        }
        else
        {
            // score for player2, but give ball to player1
            currentBallSpawn = ballSpawnPlayer1;

            if (skipScoring) {
                return;
            }
            ScoreManager.Singleton.PointToPlayer2Request();
        }
    }

    private void RespawnBall() {

        ResetBounces();

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
        if (m_networkPhysicsInteractable.IsOwner) {
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
            bounces++;

            if (bounces > maxBounces)
            {
                UpdateScore();
                RespawnBall();                
            }
        }

        
        if (collision.gameObject.CompareTag("BoundingBox"))
        {
            UpdateScore(true); // skip scoring
            RespawnBall();   
        }
    }
}
