using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

// Custom teleportation area script to implement individual features such as preventing teleporting too close to the interactor's current position.
// Currently barely used, but can be useful for future teleportation mechanics.
public class TeleportationCourt : TeleportationArea
{
    [SerializeField] private float minDistance = 0f; // Minimum allowed teleport distance

    // Overrides the teleport request generation to block teleports that are too close to the interactor.
    // parameters are inserted into the base class method GenerateTeleportRequest.
    // returns true if teleport request is valid and far enough, otherwise false.
    // XR Interaction Toolkit handles the teleportation logic if true is returned.
    protected override bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
    {
        // Call base logic to generate the teleport request
        bool success = base.GenerateTeleportRequest(interactor, raycastHit, ref teleportRequest);

        // Block teleport if the target point is too close to the interactor
        if (Vector3.Distance(raycastHit.point, interactor.transform.position) < minDistance)
        {
            return false; // always disallow teleporting too close
        }

        return success; // else return regular teleport request generation state
    }

    // Prevents selection of the teleportation area if the target is too close to the interactor.
    // paramter is inserted into the base class method IsSelectableBy.
    // returns true if selectable, otherwise false.
    // return false makes the VR pointer red, true keeps the VR pointer white to indicate that the teleportation area is selectable.
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        // Use base logic to determine if selectable
        bool success = base.IsSelectableBy(interactor);

        // If the interactor is a ray and has a valid hit, check the distance
        if (interactor is XRRayInteractor rayInteractor &&
            rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit raycastHit))
        {
            if (Vector3.Distance(raycastHit.point, interactor.transform.position) < minDistance)
            {
                return false; // always disallow teleporting too close
            }
        }

        return success; // else return regular selectable state
    }
}
