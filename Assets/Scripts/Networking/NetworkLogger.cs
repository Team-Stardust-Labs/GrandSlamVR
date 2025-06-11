using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkLogger : MonoBehaviour
{
    private void Awake()
    {
        // Optional: Set log level for more detailed Netcode logging
        NetworkManager.Singleton.LogLevel = LogLevel.Developer;

        // Subscribe to Netcode events
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;

        // Log local startup state
        CustomDebugLog.Singleton.LogNetworkManager("[NetworkLogger] NetworkManager initialized.");
    }

    private void OnDestroy()
    {
        // Clean up to prevent memory leaks
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
    }

    private void OnClientConnected(ulong clientId)
    {
        CustomDebugLog.Singleton.LogNetworkManager($"[NetworkLogger] Client connected: {clientId}");

        if (NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            CustomDebugLog.Singleton.LogNetworkManager("[NetworkLogger] You are the host.");
        }
        else if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            CustomDebugLog.Singleton.LogNetworkManager("[NetworkLogger] You are the client.");
        }
        else
        {
            CustomDebugLog.Singleton.LogNetworkManager("[NetworkLogger] A remote client joined.");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        CustomDebugLog.Singleton.LogNetworkManager($"[NetworkLogger] Client disconnected: {clientId}");
    }

    private void OnServerStarted()
    {
        CustomDebugLog.Singleton.LogNetworkManager("[NetworkLogger] Server started.");
    }

    private void OnTransportFailure()
    {
        CustomDebugLog.Singleton.LogNetworkManager("[NetworkLogger] Transport failure occurred.");
    }
}
