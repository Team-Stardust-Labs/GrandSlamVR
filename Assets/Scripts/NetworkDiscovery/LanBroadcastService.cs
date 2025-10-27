/*
Overview:
LanBroadcastService periodically sends a UDP broadcast to announce a host on the local network. It:
  • Configures a UdpClient for broadcast in Start()
  • Uses Update() to trigger broadcasts at a set interval
  • Encodes a customizable message and port, with basic error handling
  • Cleans up the socket on destroy
*/

using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LanBroadcastService : MonoBehaviour
{
    // Port and payload settings for the broadcast
    public int broadcastPort = 47777;
    public float broadcastInterval = 1f;
    public string broadcastMessage = "NGO_HOST";

    private float timer;
    private UdpClient udpClient;

    void Start()
    {
        // Initialize UDP client for broadcasting
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
    }

    void Update()
    {
        // Accumulate time and send broadcast when interval elapses
        timer += Time.deltaTime;
        if (timer >= broadcastInterval)
        {
            timer = 0f;
            Broadcast();
        }
    }

    void Broadcast()
    {
        try
        {
            // Prepare broadcast endpoint and message bytes
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
            byte[] data = Encoding.UTF8.GetBytes(broadcastMessage);
            udpClient.Send(data, data.Length, endPoint);
            CustomDebugLog.Singleton.Log("Host: Broadcasting to " + endPoint.Address.ToString());
        }
        catch (SocketException ex)
        {
            // Log any send failures
            CustomDebugLog.Singleton.Log("Broadcast failed: " + ex.Message);
        }
    }

    void OnDestroy()
    {
        // Ensure socket is closed when service is destroyed
        udpClient?.Close();
    }
}
