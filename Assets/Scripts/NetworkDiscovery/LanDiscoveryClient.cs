using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class LanDiscoveryClient : MonoBehaviour
{
    public int listenPort = 47777;
    public string expectedMessage = "NGO_HOST";

    private UdpClient udpClient;
    private Thread listenThread;
    private bool running = true;

    public string foundAddress;

    void Start()
    {
        listenThread = new Thread(ListenForBroadcast);
        listenThread.IsBackground = true;
        listenThread.Start();
    }

    void ListenForBroadcast()
    {
        udpClient = new UdpClient(listenPort);
        while (running)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, listenPort);
                byte[] data = udpClient.Receive(ref remoteEP);
                string message = Encoding.UTF8.GetString(data);
                CustomDebugLog.Singleton.Log("Got message: " + remoteEP.Address.ToString() + " - Message: " + message);
                if (message == expectedMessage)
                {
                    foundAddress = remoteEP.Address.ToString();
                    CustomDebugLog.Singleton.Log("Found Host at: " + foundAddress);
                }
            }
            catch { }
        }
    }

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
        running = false;
        udpClient?.Close();
        listenThread?.Abort();
    }
}
