using UnityEngine;

public class FinalScreen : MonoBehaviour
{
    public GameObject endScreenCanvas;
    public GameObject bluewin;
    public GameObject redwin;
    public SpectatorManager spec; 
    public ScoreManager scoreManager; 

    private float distance = 7f;

    void Start()
    {
        endScreenCanvas.SetActive(false);
        bluewin.SetActive(false);
        redwin.SetActive(false);
    }

    void Update()
    {
        if (ScoreManager.isGameFinished())
        {
            if (!SpectatorManager.isSpectator())
            {
                showEndScreenPlayer();
            }
            else 
            { 
                // Spectator End Screen potentiell anf�gen
                return;
            }
        }
    }

    void showEndScreenPlayer()
    {
        Camera cam = Camera.main;

        endScreenCanvas.SetActive(true);

        if (scoreManager.isBlueWinner())
        {
            bluewin.SetActive(true);
        }
        else
        {
            redwin.SetActive(true);
        }

        // Gr��e berechnen
        float height = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad)*0.6f;
        float width = height * cam.aspect;

        // Statt sizeDelta: lokal skalieren
        float referenceWidth = 1920f;
        float referenceHeight = 1080f;

        float scaleX = width / referenceWidth;
        float scaleY = height / referenceHeight;
        float scale = Mathf.Min(scaleX, scaleY);

        endScreenCanvas.transform.localScale = Vector3.one * scale;

        // Canvas setzen
        //RectTransform rt = endScreenCanvas.GetComponent<RectTransform>();
        //rt.sizeDelta = new Vector2(width, height);

        Vector3 forward = cam.transform.forward;
        Vector3 spawnPosition = cam.transform.position + forward * distance;
        endScreenCanvas.transform.position = spawnPosition;
        endScreenCanvas.transform.LookAt(cam.transform);
        endScreenCanvas.transform.Rotate(0, 180, 0);
    }
}
