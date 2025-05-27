using Unity.VisualScripting;
using UnityEngine;
using XRMultiplayer;

public class BallIndicator : MonoBehaviour
{
    [SerializeField] private float GroundY = 0;

    [SerializeField] private BallScoring ball;

    private Transform baseShadow;
    private Transform heightRing;

    private SpriteRenderer baseSprite;
    private SpriteRenderer heightSprite;

    private float maxHeight = 50; // Height that shows the maximum Indicator size

    void Start()
    {
        baseShadow = transform.Find("BallIndicatorBase");
        heightRing = transform.Find("BallIndicatorRing");

        baseSprite = baseShadow.GetComponent<SpriteRenderer>();
        heightSprite = heightRing.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector3 ballPos = ball.transform.position;

        if (ball.m_networkPhysicsInteractable.isThrown != true)
        {
            baseSprite.enabled = false;
            heightSprite.enabled = false;
        }

        if (ball.m_networkPhysicsInteractable.isThrown == true)
        {
            baseSprite.enabled = true;
            heightSprite.enabled = true;

            if (ball != null)
            {
                transform.position = new Vector3(ballPos.x, GroundY, ballPos.z);
            }
        }

        float newScale = Mathf.Max(0f, Mathf.Min(1f, (2* Mathf.Log10(1 + ballPos.y * 9))/5));
        // float newScale = Mathf.Min(1, (ballPos.y / maxHeight)); // All Vector Coordinates set to the Fraction of the Ball Height
        heightRing.localScale = new Vector3(newScale, newScale, newScale);
    }
}
