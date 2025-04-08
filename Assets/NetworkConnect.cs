using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkConnect : MonoBehaviour
{
    public void Create() {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host Created");
    }

    public void Join() {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client Joined");
    }
}
