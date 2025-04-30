using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkPhysicsTransformClient : NetworkBehaviour
{
    [SerializeField] private float positionLerpSpeed = 20f;

    protected Rigidbody m_Rigidbody;
    
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    
    //private NetworkVariable<bool> networkUseGravity = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        // Get associated required components
        if (!TryGetComponent(out m_Rigidbody))
        {
            CustomDebugLog.Singleton.LogNetworkManager("N-TRANSFORM CLIENT: Missing Components! Disabling Now.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        /*networkUseGravity.OnValueChanged += (oldVal, newVal) =>
        {
            if (m_Rigidbody != null) m_Rigidbody.useGravity = newVal;
        };*/
    }

    private void FixedUpdate()
    {

        m_Rigidbody.useGravity = IsOwner;

        if (IsOwner)
        {
            // Send position
            networkPosition.Value = transform.position;

            // Sync kinematic/gravity settings
            if (m_Rigidbody != null)
            {
                /*
                if (networkUseGravity.Value != m_Rigidbody.useGravity)
                    networkUseGravity.Value = m_Rigidbody.useGravity;*/
            }
        }
        else
        {
            // Interpolate using Rigidbody physics
            if (m_Rigidbody != null)
            {
                Vector3 newPos = Vector3.Lerp(m_Rigidbody.position, networkPosition.Value, Mathf.Clamp(Time.fixedDeltaTime * positionLerpSpeed, 0.0f, 1.0f));
                m_Rigidbody.MovePosition(newPos);
            }
        }
    }

}
