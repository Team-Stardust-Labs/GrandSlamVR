using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonComboGotoTutorial : ButtonCombo
{

    protected override void TriggerEvent()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
