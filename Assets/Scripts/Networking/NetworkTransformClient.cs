/*
Overview:
NetworkTransformClient customizes NetworkTransform to disable server authority in a P2P setup. It:
  • Inherits from Unity.Netcode.Components.NetworkTransform
  • Exposes a toggle to control server-authoritativeness
  • Overrides authority check to honor the isServerAuthoritative flag
*/

using UnityEngine;
using Unity.Netcode.Components;

[DisallowMultipleComponent]
public class NetworkTransformClient : NetworkTransform
{
    // Toggle to disable server authority when using peer-to-peer networking
    [SerializeField] bool isServerAuthoritative = false;

    // Override to return our custom authority setting
    protected override bool OnIsServerAuthoritative() {
        return isServerAuthoritative;
    }
}
