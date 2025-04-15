using Unity.Netcode;
using UnityEngine;

namespace XRMultiplayer
{
    public class CustomNetworkTransformClient : NetworkBehaviour
    {
        [SerializeField]
        private float positionLerpSpeed = 10f;

        private NetworkVariable<Vector3> networkedPosition =
            new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);

        private Transform cachedTransform;

        private void Awake()
        {
            cachedTransform = transform;
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // Optionally start sending position right away
                networkedPosition.Value = cachedTransform.position;
            }
        }

        private void Update()
        {
            if (IsOwner)
            {
                // Owner sends its current position to the server
                if (Vector3.Distance(networkedPosition.Value, cachedTransform.position) > 0.001f)
                {
                    networkedPosition.Value = cachedTransform.position;
                }
            }
            else
            {
                // Non-owners interpolate towards the synced position
                cachedTransform.position = Vector3.Lerp(
                    cachedTransform.position,
                    networkedPosition.Value,
                    Time.deltaTime * positionLerpSpeed
                );

                
            }
        }
    }
}
