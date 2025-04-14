using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkGrabInteractable : XRGrabInteractable
{
    private NetworkObject networkObject;
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        networkObject = GetComponent<NetworkObject>();
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // If the local player has ownership, make the Rigidbody kinematic
        if (rb && networkObject.IsOwner)
        {
            rb.isKinematic = true;
        }

        // If the object is not owned by the local player and we're a client, request ownership
        if (networkObject && !networkObject.IsOwner && NetworkManager.Singleton.IsClient)
        {
            RequestGrabOwnershipServerRpc(networkObject.NetworkObjectId);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // If the local player has ownership, release the Rigidbody to physics
        if (rb && networkObject.IsOwner)
        {
            rb.isKinematic = false;

            // Apply the velocity based on the interactor's velocity when releasing
            if (args.interactorObject is IXRInteractor interactor)
            {
                var attachTransform = interactor.GetAttachTransform(this);
                var interactorRigidbody = attachTransform.GetComponentInParent<Rigidbody>();

                if (interactorRigidbody != null)
                {
                    // Apply velocity and angular velocity
                    rb.velocity = interactorRigidbody.velocity;
                    rb.angularVelocity = interactorRigidbody.angularVelocity;

                    // Sync the velocity across the network
                    if (networkObject.IsOwner)
                    {
                        var networkRigidbody = GetComponent<NetworkRigidbodyP2P>();
                        if (networkRigidbody != null)
                        {
                            networkRigidbody.ApplyThrowVelocity(rb.velocity, rb.angularVelocity);
                        }
                    }
                }
            }
        }

        // Optionally return ownership to the host (if needed)
        //if (networkObject && networkObject.IsOwner)
        //{
        //    ReleaseOwnershipServerRpc(networkObject.NetworkObjectId);
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestGrabOwnershipServerRpc(ulong objectId, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var netObj))
        {
            netObj.ChangeOwnership(rpcParams.Receive.SenderClientId);
        }
    }

    // Optional: Release ownership back to the host
    [ServerRpc(RequireOwnership = false)]
    private void ReleaseOwnershipServerRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var netObj))
        {
            netObj.RemoveOwnership(); // Back to host
        }
    }
}
