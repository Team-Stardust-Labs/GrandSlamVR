using UnityEngine;
using Unity.XR.CoreUtils;
using System.Collections;

public class HeightRecalibrator : MonoBehaviour
{
    public XROrigin xrOrigin;

    void Start()
    {
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>();
        }
        
        RecalibrateFloorHeight();
    }

    public void RecalibrateFloorHeight()
    {
        if (xrOrigin != null)
        {
            StartCoroutine(RecalibrateCoroutine());
        }
    }

    private IEnumerator RecalibrateCoroutine()
    {
        xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;

        yield return null;

        xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
    }
}