using UnityEngine;

public class SpacialSphereHeightChecker : MonoBehaviour
{
    public GameObject player;
    public GameObject sphere;
    public float heightThreshold = 0.5f; // Height threshold relative to the player

    public void Update()
    {
        float absoluteThreshold = player.transform.position.y + heightThreshold;

        if (sphere.transform.position.y > absoluteThreshold)
        {
            // Visible
            sphere.GetComponent<Renderer>().enabled = true;
        }
        else
        {
            // Invisible
            sphere.GetComponent<Renderer>().enabled = false;
        }
    }
}
