using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles player color assignment and stores the selection in PlayerPrefs.
/// Also provides utility methods for retrieving and setting the player color,
/// and for switching to the gameplay scene.
/// </summary>
public class AssignPlayerColor : MonoBehaviour
{
    /// <summary>
    /// Enum representing possible player colors.
    /// Blue is also used for the Host.
    /// </summary>
    public enum PlayerColor
    {
        Blue, // Blue is also the Host
        Red,
        None
    }

    /// <summary>
    /// Checks if the current player color is Blue.
    /// </summary>
    /// <returns>True if player color is Blue, otherwise false.</returns>
    public static bool isBlue()
    {
        return PlayerPrefs.GetInt("PlayerColor") == (int)PlayerColor.Blue;
    }

    /// <summary>
    /// Converts an integer to the corresponding PlayerColor enum value.
    /// </summary>
    /// <param name="color">Integer representing the color.</param>
    /// <returns>PlayerColor enum value.</returns>
    public PlayerColor getPlayerColorFromInt(int color)
    {
        return (PlayerColor)color;
    }

    /// <summary>
    /// Retrieves the current player color from PlayerPrefs.
    /// </summary>
    /// <returns>PlayerColor enum value.</returns>
    public static PlayerColor getPlayerColor()
    {
        return (PlayerColor)PlayerPrefs.GetInt("PlayerColor");
    }

    /// <summary>
    /// Sets the player color to Blue, sets the run mode to VR,
    /// saves the preferences, and logs the selection.
    /// </summary>
    public void selectBlueColor()
    {
        PlayerPrefs.SetInt("PlayerColor", (int)PlayerColor.Blue);
        PlayerPrefs.SetString("RunMode", "VR");
        PlayerPrefs.Save();

        CustomDebugLog.Singleton.Log($"Player Color is set to {PlayerPrefs.GetInt("PlayerColor")}, which is {getPlayerColorFromInt(PlayerPrefs.GetInt("PlayerColor"))}");
    }

    /// <summary>
    /// Sets the player color to Red, sets the run mode to VR,
    /// saves the preferences, and logs the selection.
    /// </summary>
    public void selectRedColor()
    {
        PlayerPrefs.SetInt("PlayerColor", (int)PlayerColor.Red);
        PlayerPrefs.SetString("RunMode", "VR");
        PlayerPrefs.Save();

        CustomDebugLog.Singleton.Log($"Player Color is set to {PlayerPrefs.GetInt("PlayerColor")}, which is {(PlayerColor)PlayerPrefs.GetInt("PlayerColor")}");
    }

    /// <summary>
    /// Loads the gameplay scene (named "TutorialScene"). 
    /// Is used by Button in ConfigScene.
    /// </summary>
    public void toGameplayScene()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
