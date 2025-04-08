using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{

    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Renderer[] meshesToDisable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner) {
            foreach (Renderer mesh in meshesToDisable) {
                mesh.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        root.position = VRRigReferences.Singleton.root.position;
        root.rotation = VRRigReferences.Singleton.root.rotation;

        head.position = VRRigReferences.Singleton.head.position;
        head.rotation = VRRigReferences.Singleton.head.rotation;

        leftHand.position = VRRigReferences.Singleton.leftHand.position;
        leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

        rightHand.position = VRRigReferences.Singleton.rightHand.position;
        rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
    }
}
