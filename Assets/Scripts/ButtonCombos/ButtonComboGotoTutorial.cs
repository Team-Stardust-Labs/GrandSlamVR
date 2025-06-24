using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonComboGotoTutorial : ButtonCombo
{

    public NetworkConnect networkConnect;

    protected override void TriggerEvent()
    {
        if (networkConnect)
        {
            // when leaving the game, terminate the connection
            networkConnect.TerminateConnection();
        }
        
        SceneManager.LoadScene("TutorialScene");
    }
}
