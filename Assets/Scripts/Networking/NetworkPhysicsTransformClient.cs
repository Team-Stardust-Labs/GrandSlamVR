/*
Overview:
NetworkPhysicsTransformClient synchronizes Rigidbody positions over Netcode for GameObjects. Key features:
 - Owners update their transform regularly to a NetworkVariable
 - Non-owners smoothly interpolate to the networked position
 - Gravity is toggled based on ownership
*/

using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkPhysicsTransformClient : NetworkBehaviour
{
    [SerializeField] private float positionLerpSpeed = 20f;

    protected Rigidbody m_Rigidbody;
    // Networked position value, written by the owner
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        // Cache Rigidbody or disable if missing
        if (!TryGetComponent(out m_Rigidbody))
        {
            CustomDebugLog.Singleton.LogNetworkManager("N-TRANSFORM CLIENT: Missing Components! Disabling Now.");
            enabled = false;
            return;
        }
    }

    private void FixedUpdate()
    {
        // Only the owner applies physics gravity
        m_Rigidbody.useGravity = IsOwner;

        if (IsOwner)
        {
            // Owner: push current position to network
            networkPosition.Value = transform.position;
        }
        else
        {
            // Non-owner: interpolate toward the received network position
            if (m_Rigidbody != null)
            {
                Vector3 target = networkPosition.Value;
                float t = Mathf.Clamp(Time.fixedDeltaTime * positionLerpSpeed, 0f, 1f);
                m_Rigidbody.MovePosition(Vector3.Lerp(m_Rigidbody.position, target, t));
            }
        }
    }
}
