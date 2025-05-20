using UnityEngine;
using UnityEngine.SceneManagement;

public class AssignPlayerColor : MonoBehaviour
{
    public enum PlayerColor
    {
        Blue,
        Red
    }

    public PlayerColor getPlayerColorFromInt (int color)
    {
        return (PlayerColor)color;
    }

    public void selectBlueColor()
    {
        PlayerPrefs.SetInt("PlayerColor", (int)PlayerColor.Blue);
        PlayerPrefs.Save();

        CustomDebugLog.Singleton.Log($"Player Color steht auf {PlayerPrefs.GetInt("PlayerColor")} das ergibt {getPlayerColorFromInt(PlayerPrefs.GetInt("PlayerColor"))}");
    }

    public void selectRedColor()
    {
        PlayerPrefs.SetInt("PlayerColor", (int)PlayerColor.Red);
        PlayerPrefs.Save();

        CustomDebugLog.Singleton.Log($"Player Color steht auf {PlayerPrefs.GetInt("PlayerColor")} das ergibt {(PlayerColor)PlayerPrefs.GetInt("PlayerColor")}");
    }

    public void toGameplayScene()
    {
        SceneManager.LoadScene("FinalMapScene");
    }
}
