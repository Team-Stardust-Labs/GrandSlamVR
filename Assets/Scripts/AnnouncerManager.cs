using UnityEngine;

public class AnnouncerMananger : MonoBehaviour
{
    public AudioSource pointBlue;
    public AudioSource pointRed;
    public AudioSource matchpointBlue;
    public AudioSource matchpointRed;
    public AudioSource winBlue;
    public AudioSource winRed;

    public void PlayPointBlue()
    {
        if (!SpectatorManager.isSpectator())
        {
            return;
        }
        pointBlue.Play();
    }

    public void PlayPointRed()
    {
        if (!SpectatorManager.isSpectator())
        {
            return;
        }
        pointRed.Play();
    }

    public void PlayMatchpointBlue()
    {
        if (!SpectatorManager.isSpectator())
        {
            return;
        }
        matchpointBlue.Play();
    }

    public void PlayMatchpointRed()
    {
        if (!SpectatorManager.isSpectator())
        {
            return;
        }
        matchpointRed.Play();
    }

    public void PlayWinBlue()
    {
        if (!SpectatorManager.isSpectator())
        {
            return;
        }
        winBlue.Play();
    }

    public void PlayWinRed()
    {
        if (!SpectatorManager.isSpectator())
        {
            return;
        }
        winRed.Play();
    }
}
