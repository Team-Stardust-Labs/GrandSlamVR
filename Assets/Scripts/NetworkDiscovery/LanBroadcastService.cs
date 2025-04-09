using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LanBroadcastService : MonoBehaviour
{
    public int broadcastPort = 47777;
    public float broadcastInterval = 1f;
    public string broadcastMessage = "NGO_HOST";

    private float timer;
    private UdpClient udpClient;

    void Start()
    {
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
    }

    void Update()
    {
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
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
            byte[] data = Encoding.UTF8.GetBytes(broadcastMessage);
            udpClient.Send(data, data.Length, endPoint);
            CustomDebugLog.Singleton.Log("Host: Broadcasting to " + endPoint.Address.ToString());
        }
        catch (SocketException ex)
        {
            CustomDebugLog.Singleton.Log("Broadcast failed: " + ex.Message);
        }
    }

    void OnDestroy()
    {
        udpClient?.Close();
    }
}
