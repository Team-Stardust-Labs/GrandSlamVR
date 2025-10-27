using UnityEngine;

public class PlayAudioAfterTimeInArea : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource; // Audio source to play
    [SerializeField] private float timeInArea = 1.5f; // Time in seconds the object must stay in the area to trigger the audio
    [SerializeField] private GameObject[] vanishObjects; // Objects to disable after audio is played
    [SerializeField] private string objectTag; // Tag of the object to detect

    private float timer = 0f;
    private bool isBallInArea = false;

    private bool alreadyPlayed = false;

    private void Update()
    {
        if (isBallInArea && !alreadyPlayed)
        {
            timer += Time.deltaTime; // Increment timer

            if (timer >= timeInArea)
            {
                PlayAudio();
                DisableObjects();
                timer = 0f;
                isBallInArea = false;
                alreadyPlayed = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Compare the object tag with the specified tag
        if (other.CompareTag(objectTag))
        {
            isBallInArea = true;
            timer = 0f;
        }
    }

    // Reset the timer and state when the object exits the area
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(objectTag))
        {
            isBallInArea = false;
            timer = 0f;
        }
    }

    private void PlayAudio()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource is not assigned.");
        }
    }

    private void DisableObjects()
    {
        if (vanishObjects != null)
        {
            foreach (GameObject obj in vanishObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}