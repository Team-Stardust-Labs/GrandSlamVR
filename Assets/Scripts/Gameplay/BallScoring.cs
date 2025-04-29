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

    private void UpdateScore() {
        // Update score 
        if (transform.position.x > 0.0f)
        {
            // score for player1
            currentBallSpawn = ballSpawnPlayer1;
            ScoreManager.Singleton.AwardPointToPlayer1();
        }
        else
        {
            // score for player2
            currentBallSpawn = ballSpawnPlayer2;
            ScoreManager.Singleton.AwardPointToPlayer2();
        }
    }

    private void RespawnBall() {

        ResetBounces();

        // Fallback spawnpoint at 0,0
        if (ballSpawnPlayer1 == null || ballSpawnPlayer2 == null)
        {
            currentBallSpawn.position = new Vector3(0, 5.0f, 0);
        }

        m_rigidbody.velocity = Vector3.zero;
        m_rigidbody.angularVelocity = Vector3.zero;
        m_rigidbody.MovePosition(currentBallSpawn.position);
        m_networkPhysicsInteractable.isThrown = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
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

        // Reset ball if ball gets stale without a bounce
        if (m_networkPhysicsInteractable.isThrown && m_rigidbody.velocity.magnitude < 0.1f)
        {
            // Free retry for the same player that had the ball
            RespawnBall();
        }
    }
}
