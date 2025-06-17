using UnityEngine;

public class PlayAudioAfterTimeInArea : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float timeInArea = 1.5f;
    [SerializeField] private GameObject[] vanishObjects;
    [SerializeField] private string objectTag;

    private float timer = 0f;
    private bool isBallInArea = false;

    private bool alreadyPlayed = false;

    private void Update()
    {
        if (isBallInArea && !alreadyPlayed)
        {
            timer += Time.deltaTime;

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
        if (other.CompareTag(objectTag))
        {
            isBallInArea = true;
            timer = 0f;
        }
    }

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