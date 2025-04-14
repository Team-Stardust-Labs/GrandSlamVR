using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkRigidbodyP2P : NetworkBehaviour
{
    private Rigidbody rb;

    private NetworkVariable<Vector3> syncedVelocity = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> syncedAngularVelocity = new NetworkVariable<Vector3>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            rb.isKinematic = false;
        }
        else
        {
            rb.isKinematic = true;
            syncedVelocity.OnValueChanged += OnVelocityChanged;
            syncedAngularVelocity.OnValueChanged += OnAngularVelocityChanged;
        }
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            // Sync our velocity to others
            syncedVelocity.Value = rb.velocity;
            syncedAngularVelocity.Value = rb.angularVelocity;
        }
        else
        {
            // Non-owners: apply synced velocities
            rb.velocity = syncedVelocity.Value;
            rb.angularVelocity = syncedAngularVelocity.Value;
        }
    }

    private void OnVelocityChanged(Vector3 oldVal, Vector3 newVal)
    {
        rb.velocity = newVal;
    }

    private void OnAngularVelocityChanged(Vector3 oldVal, Vector3 newVal)
    {
        rb.angularVelocity = newVal;
    }

    // Call this method from your XRGrabInteractable on release (OnSelectExited)
    public void ApplyThrowVelocity(Vector3 throwVelocity, Vector3 throwAngularVelocity)
    {
        if (IsOwner)
        {
            rb.velocity = throwVelocity;
            rb.angularVelocity = throwAngularVelocity;

            // Update the synced values
            syncedVelocity.Value = rb.velocity;
            syncedAngularVelocity.Value = rb.angularVelocity;
        }
    }
}
