/*
Overview:
NetworkConnect manages starting and joining LAN games with Unity Netcode. It:
  • Initializes discovery objects and logs local IP on Start
  • Determines host vs. join roles based on player color
  • Configures and starts hosting with UnityTransport
  • Repeatedly attempts to join discovered hosts
  • Provides cleanup of discovery objects and network shutdown
*/

using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;

public class NetworkConnect : MonoBehaviour
{
    // Objects controlling discovery behavior
    public GameObject hostDiscoveryObject; // Has LanBroadcastService
    public GameObject joinDiscoveryObject; // Has LanDiscoveryClient + ConnectToDiscoveredHost

    public MapManager mapManager;

    private void Start()
    {
        // Ensure discovery is inactive at launch
        hostDiscoveryObject.SetActive(false);
        joinDiscoveryObject.SetActive(false);

        CustomDebugLog.Singleton.Log("NetworkConnect started");

        // Log local network address for debugging
        string localIP = GetLocalIPAddress();
        CustomDebugLog.Singleton.Log("Local IP Address: " + localIP);

        // Log assigned team color
        CustomDebugLog.Singleton.Log("Player Color: " + AssignPlayerColor.getPlayerColor());
    }

    public void StartGame()
    {
        // Blue hosts (server), Red joins (client)
        if (AssignPlayerColor.isBlue())
        {
            HostGame();
        }
        else
        {
            JoinGame();
        }
    }

    public void HostGame()
    {
        CustomDebugLog.Singleton.Log("Starting as host...");
        hostDiscoveryObject.SetActive(true);     // Enable broadcasting

        // Configure transport to listen on all interfaces
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", 7777);
        transport.ConnectionData.ServerListenAddress = "0.0.0.0";

        NetworkManager.Singleton.StartHost();     // Start host (server + client)

        mapManager.joinTeam1();                   // Assign host to team 1
    }

    public void JoinGame()
    {
        CustomDebugLog.Singleton.Log("Joining game...");
        joinDiscoveryObject.SetActive(true);     // Enable discovery listener
        InvokeRepeating(nameof(TryJoin), 2f, 2f); // Retry every 2 seconds
    }

    void TryJoin()
    {
        var connector = joinDiscoveryObject.GetComponent<ConnectToDiscoveredHost>();
        if (!string.IsNullOrEmpty(connector.discovery.foundAddress))
        {
            CustomDebugLog.Singleton.Log("Connecting to " + connector.discovery.foundAddress);
            if (connector.TryConnect())
            {
                mapManager.joinTeam2();            // Assign joining client to team 2
            }
            CancelInvoke(nameof(TryJoin));        // Stop retries once connected
        }
    }

    string GetLocalIPAddress()
    {
        // Iterate host addresses to find IPv4
        foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
                return address.ToString();
        }
        return "Not Available";
    }

    public void TerminateConnection()
    {
        CustomDebugLog.Singleton.Log("Terminating network connection...");

        // Disable any active discovery objects
        if (hostDiscoveryObject.activeSelf)
            hostDiscoveryObject.SetActive(false);
        if (joinDiscoveryObject.activeSelf)
            joinDiscoveryObject.SetActive(false);

        // Shutdown Netcode based on role
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            CustomDebugLog.Singleton.Log("Host shut down.");
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            CustomDebugLog.Singleton.Log("Client disconnected.");
        }
    }
}
