using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarHit : MonoBehaviour
{
    private AudioSource m_AudioSource;

    private Color originalColor;

    private Material objectMaterial;

    public float colorChangeDuration = 0.2f;

    public Color hitColor = Color.green;

    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
        objectMaterial = GetComponent<Renderer>().material;
        originalColor = objectMaterial.color;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            m_AudioSource.Play();
            objectMaterial.color = hitColor;
            StartCoroutine(RevertColor());
        }
    }

    IEnumerator RevertColor()
    {
        yield return new WaitForSeconds(colorChangeDuration);
        objectMaterial.color = originalColor;
    }
}
