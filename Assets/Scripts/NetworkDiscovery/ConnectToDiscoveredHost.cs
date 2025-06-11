using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class ConnectToDiscoveredHost : MonoBehaviour
{
    public LanDiscoveryClient discovery;

    public void TryConnect()
{
    if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
    {
        CustomDebugLog.Singleton.Log("ERROR: NetworkManager is already running.");
        return;
    }

    if (!string.IsNullOrEmpty(discovery.foundAddress))
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(discovery.foundAddress, 7777);

        CustomDebugLog.Singleton.Log("Trying to connect to " + discovery.foundAddress);

        if (NetworkManager.Singleton.StartClient())
        {
            CustomDebugLog.Singleton.Log("SUCCESS: Client started.");
        }
        else
        {
            CustomDebugLog.Singleton.Log("ERROR: Failed to start client.");
        }
    }
    else
    {
        CustomDebugLog.Singleton.Log("ERROR: No discovered host IP found.");
    }
}

}
