using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

/// <summary>
/// Custom teleportation area that prevents teleporting too close to the interactor's current position.
/// Currently barely used, but can be useful for future teleportation mechanics.
/// </summary>
public class TeleportationCourt : TeleportationArea
{
    [SerializeField] private float minDistance = 0f; // Minimum allowed teleport distance

    /// <summary>
    /// Overrides the teleport request generation to block teleports that are too close to the interactor.
    /// </summary>
    /// <param name="interactor">The XR interactor requesting teleportation.</param>
    /// <param name="raycastHit">The hit point for the teleportation.</param>
    /// <param name="teleportRequest">The teleport request to be filled.</param>
    /// <returns>True if teleport request is valid and far enough, otherwise false.</returns>
    protected override bool GenerateTeleportRequest(IXRInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
    {
        // Call base logic to generate the teleport request
        bool success = base.GenerateTeleportRequest(interactor, raycastHit, ref teleportRequest);

        // Block teleport if the target point is too close to the interactor
        if (Vector3.Distance(raycastHit.point, interactor.transform.position) < minDistance)
        {
            return false;
        }

        return success;
    }

    /// <summary>
    /// Prevents selection of the teleportation area if the target is too close to the interactor.
    /// </summary>
    /// <param name="interactor">The XR select interactor.</param>
    /// <returns>True if selectable, otherwise false.</returns>
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
                return false;
            }
        }

        return success;
    }
}
