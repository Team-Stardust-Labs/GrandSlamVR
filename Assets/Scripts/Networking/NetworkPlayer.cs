/*
Overview:
NetworkPlayer represents a VR player's networked avatar, syncing head and hand transforms and handling local visibility & team coloring. It:
 - Disables local meshes for the owning client
 - Assigns team color based on client ID
 - Updates transform positions & rotations each frame for owner
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    // References for avatar hierarchy
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Renderer[] meshesToDisable;

    [Header("Team Materials")]
    public Material redMaterial;
    public Material blueMaterial;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            // Hide own body meshes to avoid seeing your own avatar
            foreach (Renderer mesh in meshesToDisable)
                mesh.enabled = false;
        }

        // Color meshes based on team: owner ID 0 = red, others = blue
        Material teamMat = (OwnerClientId == 0) ? redMaterial : blueMaterial;
        foreach (Renderer mesh in meshesToDisable)
            mesh.material.SetColor("_Color", teamMat.color);
    }

    void Update()
    {
        if (!IsOwner)
            return; // Only the owner updates its own transforms

        // Continuously sync local XR rig transforms to networked avatar
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

