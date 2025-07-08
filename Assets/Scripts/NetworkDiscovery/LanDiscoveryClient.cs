/*
Overview:
LanDiscoveryClient listens for UDP broadcasts to discover a LAN host. It:
  • Runs a background thread to receive broadcast messages
  • Filters messages to match an expected payload
  • Stores the discovered host IP for connection attempts
  • Provides cleanup and shutdown of the listener and Netcode client
*/

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class LanDiscoveryClient : MonoBehaviour
{
    // Listening port and expected discovery message
    public int listenPort = 47777;
    public string expectedMessage = "NGO_HOST";

    private UdpClient udpClient;
    private Thread listenThread;
    private bool running = true;

    // Holds the IP address of the first valid host found
    public string foundAddress;

    void Start()
    {
        // Launch listener on a background thread
        listenThread = new Thread(ListenForBroadcast);
        listenThread.IsBackground = true;
        listenThread.Start();
    }

    void ListenForBroadcast()
    {
        // Bind to the listen port
        udpClient = new UdpClient(listenPort);
        while (running)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, listenPort);
                byte[] data = udpClient.Receive(ref remoteEP);
                string message = Encoding.UTF8.GetString(data);
                CustomDebugLog.Singleton.Log("Got message: " + remoteEP.Address + " - Message: " + message);

                // Check for expected discovery payload
                if (message == expectedMessage)
                {
                    foundAddress = remoteEP.Address.ToString();
                    CustomDebugLog.Singleton.Log("Found Host at: " + foundAddress);
                }
            }
            catch { /* Silently ignore exceptions */ }
        }
    }

    // Stops listening and shuts down Netcode if active
    public void StopListening()
    {
        running = false;

        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (udpClient != null)
        {
            udpClient.Close();
            udpClient.Dispose();
            udpClient = null;
        }
    }

    void OnDestroy()
    {
        // Ensure listener stops and resources are cleaned up
        running = false;
        udpClient?.Close();
        listenThread?.Abort();
    }
}
