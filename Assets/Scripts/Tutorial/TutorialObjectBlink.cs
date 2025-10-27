using UnityEngine;

public class TutorialObjectBlink : MonoBehaviour
{
    [SerializeField] private Color blinkColor = Color.red;

    void Start()
    {
        StartBlinking();
    }

    public void StartBlinking()
    {
        // Start the blinking effect
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = blinkColor;
        }

        // Fade in and out until turned off manually
        StartCoroutine(BlinkCoroutine());
    }

    private System.Collections.IEnumerator BlinkCoroutine()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("TutorialObjectBlink: No Renderer found on this GameObject. Blinking will not work.");
            yield break; // Exit the coroutine if no renderer is found
        }

        while (true)
        {
            // Fade in
            for (float t = 0; t < 1; t += Time.deltaTime)
            {
                renderer.material.color = Color.Lerp(Color.clear, blinkColor, t);
                yield return null;
            }

            // Fade out
            for (float t = 0; t < 1; t += Time.deltaTime)
            {
                renderer.material.color = Color.Lerp(blinkColor, Color.clear, t);
                yield return null;
            }
        }
    }
}
