using UnityEngine;
using UnityEngine.SceneManagement;

// Handles player color assignment and stores the selection in PlayerPrefs.
// Also provides utility methods for retrieving and setting the player color,
// and for switching to the gameplay scene.
public class AssignPlayerColor : MonoBehaviour
{
    // Enum representing possible player colors.
    public enum PlayerColor
    {
        Blue,   // Blue is also the Host
        Red,    // Red is also the Client
        None
    }

    // Checks if the current player color is Blue.
    // returns True if player color is Blue, otherwise false.
    public static bool isBlue()
    {
        return PlayerPrefs.GetInt("PlayerColor") == (int)PlayerColor.Blue;
    }

    // Converts an integer to the corresponding PlayerColor enum value.
    // Integer representing the color. 0 is Blue, 1 is Red, and 2 is None. 
    // returns PlayerColor enum value.
    public PlayerColor getPlayerColorFromInt(int color)
    {
        return (PlayerColor)color;
    }

    // Retrieves the current player color from PlayerPrefs.
    // returns PlayerColor enum value.
    public static PlayerColor getPlayerColor()
    {
        return (PlayerColor)PlayerPrefs.GetInt("PlayerColor");
    }

    // Sets the player color to Blue, sets the run mode to VR,
    // saves the preferences, and logs the selection to the visible screen in ConfigScene.
    public void selectBlueColor()
    {
        PlayerPrefs.SetInt("PlayerColor", (int)PlayerColor.Blue);
        PlayerPrefs.SetString("RunMode", "VR");
        PlayerPrefs.Save();

        CustomDebugLog.Singleton.Log($"Player Color is set to {PlayerPrefs.GetInt("PlayerColor")}, which is {getPlayerColorFromInt(PlayerPrefs.GetInt("PlayerColor"))}");
    }

    // Sets the player color to Red, sets the run mode to VR,
    // saves the preferences, and logs the selection to the visible screen in ConfigScene.
    public void selectRedColor()
    {
        PlayerPrefs.SetInt("PlayerColor", (int)PlayerColor.Red);
        PlayerPrefs.SetString("RunMode", "VR");
        PlayerPrefs.Save();

        CustomDebugLog.Singleton.Log($"Player Color is set to {PlayerPrefs.GetInt("PlayerColor")}, which is {(PlayerColor)PlayerPrefs.GetInt("PlayerColor")}");
    }

    // Loads the Tutorial scene. 
    // Is used by Button in ConfigScene if finished with configuring the player color.
    public void toGameplayScene()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
