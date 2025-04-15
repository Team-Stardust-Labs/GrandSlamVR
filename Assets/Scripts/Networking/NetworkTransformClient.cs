using UnityEngine;
using Unity.Netcode.Components;

[DisallowMultipleComponent]
public class NetworkTransformClient : NetworkTransform
{
    // We use P2P and not Client-Server so we need to disable server authority for network transforms

    [SerializeField] bool isServerAuthoritative = false;
    protected override bool OnIsServerAuthoritative() {
        return isServerAuthoritative;
    }
}
