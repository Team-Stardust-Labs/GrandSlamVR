using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupScript : MonoBehaviour
{
    public string configKey = "PlayerColor";
    public string configSceneName = "ConfigScene";
    public string gameplaySceneName = "FinalMapScene";

    void Start()
    {
        if (PlayerPrefs.HasKey(configKey))
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            SceneManager.LoadScene(configSceneName);
        }
    }
}
