using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;

public class NetworkTransformClient : NetworkTransform
{
    // We use P2P and not Client-Server so we need to disable server authority for network transforms
    protected override bool OnIsServerAuthoritative() {
        return false;
    }
}
