using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToScene : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    public void NextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
