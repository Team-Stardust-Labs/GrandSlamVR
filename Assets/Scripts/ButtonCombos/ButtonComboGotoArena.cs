using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonComboGotoArena : ButtonCombo
{
    protected override void TriggerEvent()
    {
        RequestDisconnectEveryoneAndLoadArenaRpc();
    }

    [Rpc(SendTo.Server)]
    protected void RequestDisconnectEveryoneAndLoadArenaRpc()
    {
        DisconnectEveryoneAndLoadArenaRpc();
    }

    // this is called only by the server 
    [Rpc(SendTo.Everyone)]
    protected void DisconnectEveryoneAndLoadArenaRpc()
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
        SceneManager.LoadScene("FinalMapScene");
    }
}