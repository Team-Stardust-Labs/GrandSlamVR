/*
Overview:
NetworkPhysicsInteractable extends NetworkBaseInteractable for XR multiplayer, integrating Unity's XR Interaction Toolkit with Netcode. Key responsibilities:
 - Controls physics ownership transfer on collisions
 - Manages spawn locking and physics reset via NetworkVariables
 - Calculates hand velocity for lasso interactions
 - Handles trail and audio effects for throws and bounces
 - Syncs state with RPC calls across clients
*/

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.XR.PXR;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace XRMultiplayer
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(BallScoring)), RequireComponent(typeof(AudioSource))]
    public class NetworkPhysicsInteractable : NetworkBaseInteractable
    {
        // Ownership transfer thresholds
        [Header("Ownership Transfer Settings")]
        [SerializeField] protected bool m_AllowCollisionOwnershipExchange = true;
        [SerializeField] protected float m_MinExchangeVelocityMagnitude = 0.025f;

        // Spawn locking via networked variable
        [Header("Spawn Options")]
        public bool spawnLocked = true;
        protected NetworkVariable<bool> m_LockedOnSpawn = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public bool lockedOnSpawn => m_LockedOnSpawn.Value;

        protected bool m_RequestingOwnership = false;
        protected Rigidbody m_Rigidbody;
        protected Collider m_Collider;

        // Reset flag for physics sync
        protected NetworkVariable<bool> m_ResettingObject = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        protected IEnumerator checkOwnershipRoutine;

        // Hand velocity calculation for lasso
        Vector3 m_PrevPos;
        Vector3 m_SmoothVelocity;
        bool m_PauseVelocityCalculations = false;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor m_CurrentInteractor;

        // Ball scoring reference
        private BallScoring m_ball_scoring;

        // Lasso-specific buffers
        [SerializeField] private int interactorFramesToCalculate = 16;
        private Vector3[] interactorVelocityHistory;
        private int interactorFrameIndex = 0;
        private Vector3 interactorPrevPosition;
        private Vector3 averageHandVelocity;
        private Vector3 lastHandVelocity;

        // Throw tracking
        public bool isThrown = false;
        public AssignPlayerColor.PlayerColor lastThrownPlayerColor = AssignPlayerColor.PlayerColor.None;

        [Header("Audio Options")]
        [SerializeField] private AudioSource m_ThrowLightAudioSource;
        [SerializeField] private AudioSource m_ThrowStrongAudioSource;
        [SerializeField] private AudioSource m_BounceSound;
        private AudioSource m_FlyingAudioSource;

        [Header("Trail Options")]
        [SerializeField] private GameObject trailPrefab;
        private List<TrailRenderer> m_TrailRenderers = new List<TrailRenderer>();

        // XR Grab reference
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

        public override void Awake()
        {
            base.Awake();

            // Cache components
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Collider = GetComponentInChildren<Collider>();
            m_ball_scoring = GetComponent<BallScoring>();
            m_FlyingAudioSource = GetComponent<AudioSource>();
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            // Prepare velocity history buffer
            interactorVelocityHistory = new Vector3[interactorFramesToCalculate];

            // Instantiate and deactivate trails
            if (trailPrefab != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject trailInstance = Instantiate(trailPrefab, transform);
                    trailInstance.transform.localRotation = Quaternion.Euler(0, i * 90, 0);
                    m_TrailRenderers.AddRange(trailInstance.GetComponentsInChildren<TrailRenderer>());
                }
                deactivateTrailsRpc(); // ensure trails start off
            }
            else
            {
                Debug.LogWarning("Trail Prefab is not assigned in the inspector.", this);
            }
        }

        [Rpc(SendTo.Everyone)]
        public void deactivateTrailsRpc()
        {
            // Turn off all trail renderers
            foreach (var trail in m_TrailRenderers)
                if (trail != null)
                    trail.emitting = false;
        }

        public void Ungrab()
        {
            // Force release from current interactor if selected
            if (grabInteractable.isSelected && grabInteractable.interactorsSelecting.Count > 0)
            {
                var interactor = grabInteractable.interactorsSelecting[0];
                var interactionManager = grabInteractable.interactionManager ?? FindObjectOfType<XRInteractionManager>();
                if (interactionManager != null)
                    interactionManager.SelectExit(interactor, grabInteractable);
                else
                    Debug.LogWarning("No XRInteractionManager found to force ungrab.");
            }
        }

        [Rpc(SendTo.Everyone)]
        public void triggerTrailsRpc(bool strongThrow)
        {
            // Activate trails differently based on throw strength
            if (strongThrow)
                foreach (var trail in m_TrailRenderers)
                    if (trail != null)
                        trail.emitting = true;
            else
                m_TrailRenderers[0].emitting = true; // fallback for light throw
        }

        void FixedUpdate()
        {
            if (m_PauseVelocityCalculations) return;

            // Update audio volumes based on speed
            m_FlyingAudioSource.volume = Mathf.Clamp(m_Rigidbody.linearVelocity.magnitude / 20.0f, 0.2f, 1.0f);
            m_BounceSound.volume = Mathf.Clamp(m_Rigidbody.linearVelocity.magnitude / 20.0f, 0.8f, 1.0f);
            m_BounceSound.pitch = 0.8f + Mathf.Clamp(m_Rigidbody.linearVelocity.magnitude / 80.0f, 0.0f, 0.5f);

            if (m_CurrentInteractor != null && isInteracting)
            {
                // Record hand velocity and adjust lasso length
                Vector3 currentPos = m_CurrentInteractor.transform.position;
                Vector3 handVel = (currentPos - interactorPrevPosition) / Time.fixedDeltaTime;
                interactorVelocityHistory[interactorFrameIndex] = handVel;
                interactorFrameIndex = (interactorFrameIndex + 1) % interactorFramesToCalculate;
                interactorPrevPosition = currentPos;
                lastHandVelocity = handVel;

                // Compute average velocity
                Vector3 total = Vector3.zero;
                for (int i = 0; i < interactorFramesToCalculate; i++) total += interactorVelocityHistory[i];
                averageHandVelocity = total / interactorFramesToCalculate;

                // Smoothly change lasso attach point
                var rayInteractor = m_CurrentInteractor as UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor;
                if (rayInteractor != null)
                {
                    float newDist = Mathf.Clamp(LassoCurve(averageHandVelocity.magnitude) * 2f, 1.0f, 10.0f);
                    float smooth = Mathf.Lerp(rayInteractor.attachTransform.localPosition.z, newDist, Time.fixedDeltaTime * 0.75f);
                    rayInteractor.attachTransform.localPosition = new Vector3(0, 0, smooth);
                }
                return;
            }

            // Reset velocity history when not interacting
            if (!isInteracting)
            {
                for (int i = 0; i < interactorFramesToCalculate; i++)
                    interactorVelocityHistory[i] = Vector3.zero;
                interactorFrameIndex = 0;
            }
        }

        // Lasso distance curve function
        float LassoCurve(float x)
        {
            if (x < 4f) return x;
            if (x < 8f) return x * 1.5f;
            return 1f + Mathf.Log(x - 6f) * 0.5f;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // Listen for physics reset signals
            m_ResettingObject.OnValueChanged += OnObjectPhysicsReset;
            if (IsOwner)
            {
                m_LockedOnSpawn.Value = spawnLocked;
                m_Rigidbody.constraints = spawnLocked ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            // Reset on despawn if needed
            if (m_ResetObjectOnDisconnect && !m_Rigidbody.isKinematic)
                ResetObjectPhysics();
        }

        protected override void OnIsInteractingChanged(bool oldValue, bool newValue)
        {
            base.OnIsInteractingChanged(oldValue, newValue);
            if (IsOwner && newValue && m_LockedOnSpawn.Value) m_LockedOnSpawn.Value = false;
            if (!newValue)
            {
                // Toggle collider to reset contact state
                m_Collider.enabled = false;
                m_Collider.enabled = true;
            }
        }

        void OnObjectPhysicsReset(bool oldValue, bool currentValue)
        {
            // Pause physics updates during reset
            m_PauseVelocityCalculations = currentValue;
        }

        public override void ResetObject()
        {
            base.ResetObject();
            ResetObjectPhysics(); // clear velocities and sync
        }

        public void ResetObjectPhysics()
        {
            // Temporarily disable interpolation and kinematic
            int origInterp = (int)m_Rigidbody.interpolation;
            bool wasKinematic = m_Rigidbody.isKinematic;
            if (!wasKinematic)
            {
                m_Rigidbody.linearVelocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }
            m_Rigidbody.interpolation = RigidbodyInterpolation.None;
            m_Rigidbody.isKinematic = true;

            // Trigger sync if owner
            if (IsOwner && NetworkManager.IsConnectedClient && !NetworkManager.Singleton.ShutdownInProgress)
                m_ResettingObject.Value = true;

            StartCoroutine(ResetPhysicsRoutine(wasKinematic, origInterp));
        }

        IEnumerator ResetPhysicsRoutine(bool wasKinematic, int interpolation)
        {
            yield return new WaitForFixedUpdate();
            m_Rigidbody.interpolation = (RigidbodyInterpolation)interpolation;
            m_Rigidbody.isKinematic = wasKinematic;
            if (IsOwner && NetworkManager.IsConnectedClient && !NetworkManager.Singleton.ShutdownInProgress)
                m_ResettingObject.Value = false;
        }

        public override void OnSelectEnteredLocal(BaseInteractionEventArgs args)
        {
            base.OnSelectEnteredLocal(args);
            // Prepare for new grab: stop audio, reset state and trails
            isThrown = false;
            m_FlyingAudioSource.Stop();
            lastThrownPlayerColor = AssignPlayerColor.PlayerColor.None;
            deactivateTrailsRpc();
            m_ball_scoring.ResetBounces();
            m_ball_scoring.ResetColor();

            // Haptic feedback on grab
            PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.BothController, 0.5f, 250, 50);

            if (m_IgnoreSocketSelectedCallback && args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
                return;

            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor interactor)
                m_CurrentInteractor = interactor;

            // Zero out velocities and disable gravity
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.useGravity = false;

            ClearHandVelocityHistory();
        }

        public override void OnSelectExitedLocal(BaseInteractionEventArgs args)
        {
            base.OnSelectExitedLocal(args);
            // Play haptics on release
            AudioClip hapticAsset = Resources.Load<AudioClip>("sfx_blowingthrow_hapticversion");
            int sourceid = 0;
            PXR_Input.SendHapticBuffer(PXR_Input.VibrateType.BothController, hapticAsset, PXR_Input.ChannelFlip.No, ref sourceid);

            m_CurrentInteractor = null;
            if (m_IgnoreSocketSelectedCallback && args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
                return;
            if (m_BaseInteractable.isSelected) return;

            // Apply throw velocity and gravity
            m_Rigidbody.linearVelocity = Mathf.Clamp(averageHandVelocity.magnitude * 20.0f, 0.025f, 80.0f) * averageHandVelocity.normalized;
            m_Rigidbody.useGravity = true;
            isThrown = true;
            m_FlyingAudioSource.Play();
            lastThrownPlayerColor = AssignPlayerColor.getPlayerColor();

            bool strongThrow = m_Rigidbody.linearVelocity.magnitude > 79.0f;
            if (strongThrow) { m_Rigidbody.linearVelocity *= 1.25f; m_ThrowStrongAudioSource.Play(); }
            else { m_ThrowLightAudioSource.Play(); }

            triggerTrailsRpc(strongThrow);
        }

        public override void OnGainedOwnership()
        {
            base.OnGainedOwnership();
            m_RequestingOwnership = false;
            m_IsInteracting.Value = baseInteractable.isSelected;
        }

        public override void OnLostOwnership()
        {
            base.OnLostOwnership();
            // Notify and reset ownership request
            m_FlyingAudioSource.Play();
            if (checkOwnershipRoutine != null) StopCoroutine(checkOwnershipRoutine);
            m_RequestingOwnership = false;
            CustomDebugLog.Singleton.LogNetworkManager($"Ownership Lost on Object {gameObject.name}");
        }

        void OnCollisionEnter(Collision collision)
        {
            // Play bounce on floor collision
            if (collision.gameObject.CompareTag("Court")) m_BounceSound.Play();

            // Exchange ownership if moving faster
            if (!IsOwner || !m_AllowCollisionOwnershipExchange) return;
            var other = collision.transform.GetComponentInParent<NetworkPhysicsInteractable>();
            if (other != null && (isInteracting || IsMovingFaster(other.m_Rigidbody)))
                other.RequestOwnership();
        }

        bool IsMovingFaster(Rigidbody other) =>
            m_Rigidbody.linearVelocity.magnitude > m_MinExchangeVelocityMagnitude &&
            m_Rigidbody.linearVelocity.magnitude > other.linearVelocity.magnitude;

        public bool OwnershipTransferBlocked() =>
            isInteracting || IsOwner || m_RequestingOwnership ||
            m_LockedOnSpawn.Value || !m_AllowCollisionOwnershipExchange ||
            !NetworkObject.IsSpawned || baseInteractable.isSelected || m_ResettingObject.Value;

        public void RequestOwnership()
        {
            if (IsOwner || OwnershipTransferBlocked()) return;
            m_RequestingOwnership = true;
            m_Rigidbody.isKinematic = false;
            RequestOwnershipRpc(NetworkManager.Singleton.LocalClientId);
            if (checkOwnershipRoutine != null) StopCoroutine(checkOwnershipRoutine);
            checkOwnershipRoutine = CheckOwnershipRoutine();
            StartCoroutine(checkOwnershipRoutine);
        }

        [Rpc(SendTo.Server)]
        void RequestOwnershipRpc(ulong clientId)
        {
            NetworkObject.ChangeOwnership(clientId);
        }

        IEnumerator CheckOwnershipRoutine()
        {
            // Wait based on RTT before timing out
            float waitTime = Mathf.Clamp(
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId) * 2f,
                0.025f, 5f);
            yield return new WaitForSeconds(waitTime);
            if (!IsOwner)
                Debug.LogWarning($"Ownership Request Timed Out on Object {gameObject.name}");
            m_RequestingOwnership = false;
        }

        void ClearHandVelocityHistory()
        {
            // Zero out buffer indices
            for (int i = 0; i < interactorFramesToCalculate; i++)
                interactorVelocityHistory[i] = Vector3.zero;
            interactorFrameIndex = 0;
            interactorPrevPosition = m_CurrentInteractor != null ? m_CurrentInteractor.transform.position : Vector3.zero;
            averageHandVelocity = Vector3.zero;
        }
    }
}
