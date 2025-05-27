using UnityEngine;

public class PlayAudioAfterTimeInArea : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float timeInArea = 1.5f;

    private float timer = 0f;
    private bool isPlayerInArea = false;

    private bool alreadyPlayed = false;

    private void Update()
    {
        if (isPlayerInArea && !alreadyPlayed)
        {
            timer += Time.deltaTime;

            if (timer >= timeInArea)
            {
                PlayAudio();
                timer = 0f;
                isPlayerInArea = false;
                alreadyPlayed = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isPlayerInArea = true;
        timer = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        isPlayerInArea = false;
        timer = 0f;
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
}