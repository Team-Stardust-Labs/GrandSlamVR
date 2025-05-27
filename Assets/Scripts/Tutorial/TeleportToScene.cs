using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToScene : MonoBehaviour
{
    // Usable as unityEvent

    [SerializeField] private string sceneToLoad;

    public void NextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
