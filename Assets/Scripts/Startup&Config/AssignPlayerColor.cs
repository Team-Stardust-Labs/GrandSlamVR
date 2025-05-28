using UnityEngine;
using UnityEngine.SceneManagement;

public class AssignPlayerColor : MonoBehaviour
{
    public enum PlayerColor
    {
        Blue, // Blue is also the Host
        Red,
        None
    }

    public static bool isBlue()
    {
        return PlayerPrefs.GetInt("PlayerColor") == (int)PlayerColor.Blue;
    }

    public PlayerColor getPlayerColorFromInt(int color)
    {
        return (PlayerColor)color;
    }

    public static PlayerColor getPlayerColor()
    {
        return (PlayerColor) PlayerPrefs.GetInt("PlayerColor");
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
        SceneManager.LoadScene("TutorialScene");
    }
}
