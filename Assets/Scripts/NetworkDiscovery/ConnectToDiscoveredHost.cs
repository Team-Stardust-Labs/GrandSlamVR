using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class ConnectToDiscoveredHost : MonoBehaviour
{
    public LanDiscoveryClient discovery;

    // returns false if connection was a failure, true if success
    public bool TryConnect()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            CustomDebugLog.Singleton.Log("ERROR: NetworkManager is already running.");
            return true; // Connection was a failure, but we don't want to retry
        }

        if (!string.IsNullOrEmpty(discovery.foundAddress))
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(discovery.foundAddress, 7777);

            CustomDebugLog.Singleton.Log("Trying to connect to " + discovery.foundAddress);

            if (NetworkManager.Singleton.StartClient())
            {
                CustomDebugLog.Singleton.Log("SUCCESS: Client started.");
                return true; // Connection was a success
            }
            else
            {
                CustomDebugLog.Singleton.Log("ERROR: Failed to start client.");
                return false; // Connection was a failure
            }
        }
        else
        {
            CustomDebugLog.Singleton.Log("ERROR: No discovered host IP found.");
        }
        
        return false; // Connection was a failure
    }


}
