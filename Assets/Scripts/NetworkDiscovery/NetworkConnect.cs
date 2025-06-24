using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class NetworkConnect : MonoBehaviour
{
    public GameObject hostDiscoveryObject; // Object with LanBroadcastService
    public GameObject joinDiscoveryObject; // Object with LanDiscoveryClient + ConnectToDiscoveredHost

    public MapManager mapManager;

    private void Start()
    {
        // Ensure the discovery objects are inactive at the start
        hostDiscoveryObject.SetActive(false);
        joinDiscoveryObject.SetActive(false);

        CustomDebugLog.Singleton.Log("NetworkConnect started");

        string localIP = GetLocalIPAddress();
        CustomDebugLog.Singleton.Log("Local IP Address: " + localIP);


        CustomDebugLog.Singleton.Log("Player Color: " + AssignPlayerColor.getPlayerColor());
    }

    public void StartGame()
    {
        // Blue hosts the game and is also the server
        if (AssignPlayerColor.isBlue())
        {
            HostGame();
        }
        else
        {
            // Red joins the game
            JoinGame();
        }
    }

    public void HostGame()
    {
        CustomDebugLog.Singleton.Log("Starting as host...");
        hostDiscoveryObject.SetActive(true);     // Start broadcasting

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", 7777); // '0.0.0.0' listens on all network interfaces
        transport.ConnectionData.ServerListenAddress = "0.0.0.0"; // Accept external connections
        NetworkManager.Singleton.StartHost(); // Start the server

        // TODO: maybe only join team with a callback on client connected
        mapManager.joinTeam1();
    }

    public void JoinGame()
    {
        CustomDebugLog.Singleton.Log("Joining game...");
        joinDiscoveryObject.SetActive(true);     // Start listening
        InvokeRepeating(nameof(TryJoin), 2f, 2f); // Try connecting every 2 seconds
    }

    void TryJoin()
    {
        var connector = joinDiscoveryObject.GetComponent<ConnectToDiscoveredHost>();
        if (!string.IsNullOrEmpty(connector.discovery.foundAddress))
        {
            CustomDebugLog.Singleton.Log("Connecting to " + connector.discovery.foundAddress);
            if (connector.TryConnect())
            {
                mapManager.joinTeam2();
            }
            CancelInvoke(nameof(TryJoin)); // we found a host so we can stop trying to join
        }
    }

    string GetLocalIPAddress()
    {
        string localIP = "Not Available";
        foreach (var address in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = address.ToString();
                break;
            }
        }
        return localIP;
    }

    // This terminates the Network Connection for Server and Client
    public void TerminateConnection()
    {
        CustomDebugLog.Singleton.Log("Terminating network connection...");

        // Stop discovery broadcasts and listeners
        if (hostDiscoveryObject.activeSelf)
            hostDiscoveryObject.SetActive(false);

        if (joinDiscoveryObject.activeSelf)
            joinDiscoveryObject.SetActive(false);

        // Stop network session based on current role
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown(); // Stops host and disconnects all clients
            CustomDebugLog.Singleton.Log("Host shut down.");
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown(); // Disconnects from the host
            CustomDebugLog.Singleton.Log("Client disconnected.");
        }
    }
}
