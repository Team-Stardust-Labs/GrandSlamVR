using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer;

public class BallScoring : MonoBehaviour
{
    private int bounces = 0;
    public Transform ballSpawnPlayer1;
    public Transform ballSpawnPlayer2;
    private Transform currentBallSpawn;

    private Rigidbody m_rigidbody;
    
    void Start()
    {
        currentBallSpawn = new GameObject("DefaultSpawn").transform;
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Court") && NetworkPhysicsInteractable.isThrown == true)
        {
            // Debug Spawn at 0,0
            if (ballSpawnPlayer1 == null || ballSpawnPlayer2 == null)
            {
                currentBallSpawn.position = new Vector3(0, 5.0f, 0);
            }
            else
            {

                if (transform.position.x > -0.0f)
                {
                    currentBallSpawn = ballSpawnPlayer1;
                }
                else
                {
                    currentBallSpawn = ballSpawnPlayer2;
                }
            }
            bounces++;

            if (bounces > 1)
            {
                bounces = 0;
                m_rigidbody.velocity = Vector3.zero;
                m_rigidbody.angularVelocity = Vector3.zero;
                m_rigidbody.MovePosition(currentBallSpawn.position);
                NetworkPhysicsInteractable.isThrown = false;
            }
        }
    }
}
