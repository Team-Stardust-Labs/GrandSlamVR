using System;
using JetBrains.Annotations;
using Unity.Cinemachine;
using Unity.XR.PICO.TOBSupport;
using UnityEngine;

public class CameraSwitching : MonoBehaviour
{
    public CinemachineCamera[] cameras;
    public float switchInterval;

    public float fastInterval;
    public int fastSlots; // number of slots that should be cut out of faster

    private float timer;

    void Start()
    {
        timer = switchInterval;

        foreach (var cam in cameras)
            cam.Priority = 0;

        SwitchToRandomCamera();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = switchInterval;
            SwitchToRandomCamera();
            
        }
    }

    public void SwitchToRandomCamera()
    {
        int rng = UnityEngine.Random.Range(0, cameras.Length);
        SwitchTo(rng);
    }

    public void SwitchTo(string cameraName)
    {
        foreach (var cam in cameras)
        {
            if (cam.name == cameraName)
            {
                cam.Priority = 10;
            }
            else
            {
                cam.Priority = 0;
            }
        }
    }

    public void SwitchTo(int cameraIndex)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (i == cameraIndex)
                cameras[i].Priority = 10;   
            else
                cameras[i].Priority = 0;
        }
        if (cameraIndex <= fastSlots-1)
        {
            timer = fastInterval;
        }
    }
}
