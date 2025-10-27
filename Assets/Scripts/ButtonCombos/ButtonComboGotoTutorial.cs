using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonComboGotoTutorial : ButtonCombo
{

    public NetworkConnect networkConnect;

    protected override void TriggerEvent()
    {
        RequestDisconnectEveryoneAndLoadTutorialRpc();
    }

    [Rpc(SendTo.Server)]
    protected void RequestDisconnectEveryoneAndLoadTutorialRpc()
    {
        DisconnectEveryoneAndLoadTutorialRpc();
    }

    // this is called only by the server 
    [Rpc(SendTo.Everyone)]
    protected void DisconnectEveryoneAndLoadTutorialRpc()
    {
        Disconnect();
    }

    // this is called locally
    protected void Disconnect()
    {
        if (SpectatorManager.isSpectator())
        {
            return;
        }

        // Load the scene first
        SceneManager.LoadScene("TutorialScene");

        // Then terminate the connection after a short delay
        if (networkConnect)
        {
            networkConnect.TerminateConnection();
        }
    }
}