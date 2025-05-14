using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class TeleportationCourt : TeleportationArea
{
    [SerializeField] private float minDistance = 2.5f;
    protected override bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
    {
        bool success = base.GenerateTeleportRequest(interactor, raycastHit, ref teleportRequest);

        if (Vector3.Distance(raycastHit.point, interactor.transform.position) < minDistance)
        {
            return false;
        }

        return success;
    }

    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        bool success = base.IsSelectableBy(interactor);

        if (interactor is XRRayInteractor rayInteractor &&
            rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit raycastHit))
        {
            if (Vector3.Distance(raycastHit.point, interactor.transform.position) < minDistance)
            {
                return false;
            }
        }

        return success;
    }

}