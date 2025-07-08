using UnityEngine;
using Unity.XR.CoreUtils;
using System.Collections;

public class HeightRecalibrator : MonoBehaviour
{
    public XROrigin xrOrigin; // Player XR Origin

    // Loaded on scene start
    void Start()
    {
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>(); // Suche XR Origin
        }
        
        RecalibrateFloorHeight();
    }

    // Trigger function
    public void RecalibrateFloorHeight()
    {
        if (xrOrigin != null)
        {
            StartCoroutine(RecalibrateCoroutine());
        }
    }

    private IEnumerator RecalibrateCoroutine()
    {
        xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device; // Tracking Origin: Device

        yield return null; // Wait for next frame

        xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor; // Tracking Origin: Floor
    }
}