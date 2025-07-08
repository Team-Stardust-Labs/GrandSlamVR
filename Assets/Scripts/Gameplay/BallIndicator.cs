using UnityEngine;

public class BallIndicator : MonoBehaviour
{
    [SerializeField] private float GroundY = 0; // Y position of the ground plane (should be 0)

    [SerializeField] private BallScoring ball; // Reference to the BallScoring script

    // Transform references for the indicator components
    private Transform baseShadow; // General shadow for ball position
    private Transform heightRing; // Ring that indicates the height of the ball

    // SpriteRenderer references for the indicator components
    private SpriteRenderer baseSprite; // General shadow for ball position
    private SpriteRenderer heightSprite; // Ring that indicates the height of the ball

    void Start()
    {
        // Assign the Indicator Object components
        baseShadow = transform.Find("BallIndicatorBase");
        heightRing = transform.Find("BallIndicatorRing");

        baseSprite = baseShadow.GetComponent<SpriteRenderer>();
        heightSprite = heightRing.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector3 ballPos = ball.transform.position; // Tracks the position of the ball

        // If the ball is not thrown, hide the indicator sprites
        if (ball.m_networkPhysicsInteractable.isThrown != true)
        {
            baseSprite.enabled = false;
            heightSprite.enabled = false;
        }

        // If the ball is thrown, show the indicator sprites and update their positions
        if (ball.m_networkPhysicsInteractable.isThrown == true)
        {
            baseSprite.enabled = true;
            heightSprite.enabled = true;

            if (ball != null)
            {
                transform.position = new Vector3(ballPos.x, GroundY, ballPos.z); // Update the position of the indicator to match the ball's position, but on ground level
            }
        }

        float newScale = Mathf.Max(0f, Mathf.Min(1f, (2* Mathf.Log10(1 + ballPos.y * 9))/5)); // Calculate the new scale of the ring based on the ball's height
        heightRing.localScale = new Vector3(newScale, newScale, newScale); // Set the ring's scale based on the calculated value
    }
}
