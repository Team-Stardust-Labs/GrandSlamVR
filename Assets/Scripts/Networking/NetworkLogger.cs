/*
Overview:
The NetworkLogger MonoBehaviour configures detailed network logging and monitors key Netcode for GameObjects events. It:
  • Sets the Netcode log level to Developer on Awake
  • Subscribes to client connect/disconnect, server start, and transport failure callbacks
  • Logs concise, informative messages via CustomDebugLog
  • Unsubscribes on destroy to prevent leaks
*/

using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkLogger : MonoBehaviour
{
    private void Awake()
    {
        // Increase Netcode verbosity for detailed diagnostics
        NetworkManager.Singleton.LogLevel = LogLevel.Developer;

        // Hook into major network lifecycle events
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;

        // Confirm initialization in our custom logger
        CustomDebugLog.Singleton.LogNetworkManager("[NetworkLogger] NetworkManager initialized.");
    }

    private void OnDestroy()
    {
        // Safely unsubscribe to avoid memory leaks or null refs on shutdown
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
    }

    private void OnClientConnected(ulong clientId)
    {
        CustomDebugLog.Singleton.LogNetworkManager($"[NetworkLogger] Client connected: {clientId}");

        // Distinguish host, local client, and remote clients
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
        // Simple disconnect notification
        CustomDebugLog.Singleton.LogNetworkManager($"[NetworkLogger] Client disconnected: {clientId}");
    }

    private void OnServerStarted()
    {
        // Notify that server is up and running
        CustomDebugLog.Singleton.LogNetworkManager("[NetworkLogger] Server started.");
    }

    private void OnTransportFailure()
    {
        // Catch and log transport-level errors
        CustomDebugLog.Singleton.LogNetworkManager("[NetworkLogger] Transport failure occurred.");
    }
}
