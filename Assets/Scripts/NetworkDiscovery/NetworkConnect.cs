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

    private void Start()
    {
        // Ensure the discovery objects are inactive at the start
        hostDiscoveryObject.SetActive(false);
        joinDiscoveryObject.SetActive(false);

        CustomDebugLog.Singleton.Log("NetworkConnect started");

        string localIP = GetLocalIPAddress();
        CustomDebugLog.Singleton.Log("Local IP Address: " + localIP);
    }

    public void HostGame()
    {
        CustomDebugLog.Singleton.Log("Starting as host...");
        hostDiscoveryObject.SetActive(true);     // Start broadcasting

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", 7777); // '0.0.0.0' listens on all network interfaces
        transport.ConnectionData.ServerListenAddress = "0.0.0.0"; // Accept external connections
        NetworkManager.Singleton.StartHost(); // Start the server
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
            connector.TryConnect();
            CancelInvoke(nameof(TryJoin));
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
}
