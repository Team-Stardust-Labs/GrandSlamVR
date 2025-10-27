/*
Overview:
ConnectToDiscoveredHost attempts to join a peer-discovered LAN host via Unity Transport. It:
 - Checks that Netcode isn't already running
 - Reads the discovered IP from LanDiscoveryClient
 - Configures UnityTransport with the found address
 - Starts the client and logs success or failure
*/

using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class ConnectToDiscoveredHost : MonoBehaviour
{
    // Dependency providing the discovered host IP
    public LanDiscoveryClient discovery;

    // Attempts to start a Netcode client; returns true on success or non-retryable failure
    public bool TryConnect()
    {
        // Prevent connecting if already running as host/client/server
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            CustomDebugLog.Singleton.Log("ERROR: NetworkManager is already running.");
            return true; // Treat as non-retryable 'failure'
        }

        // Ensure we have a discovered address
        if (!string.IsNullOrEmpty(discovery.foundAddress))
        {
            // Configure transport to use the discovered host IP and default port
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(discovery.foundAddress, 7777);

            CustomDebugLog.Singleton.Log("Trying to connect to " + discovery.foundAddress);

            // Attempt to start the client
            if (NetworkManager.Singleton.StartClient())
            {
                CustomDebugLog.Singleton.Log("SUCCESS: Client started.");
                return true;
            }
            else
            {
                CustomDebugLog.Singleton.Log("ERROR: Failed to start client.");
                return false;
            }
        }
        else
        {
            // No host found to connect to
            CustomDebugLog.Singleton.Log("ERROR: No discovered host IP found.");
        }
        
        return false; // Final failure fallback
    }
}
