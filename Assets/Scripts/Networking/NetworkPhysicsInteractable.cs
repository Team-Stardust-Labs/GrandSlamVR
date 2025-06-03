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
        [Header("Ownership Transfer Settings")]
        [SerializeField] protected bool m_AllowCollisionOwnershipExchange = true;
        [SerializeField] protected float m_MinExchangeVelocityMagnitude = 0.025f;

        [Header("Spawn Options")]
        public bool spawnLocked = true;
        protected NetworkVariable<bool> m_LockedOnSpawn = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public bool lockedOnSpawn => m_LockedOnSpawn.Value;

        protected bool m_RequestingOwnership = false;
        protected Rigidbody m_Rigidbody;
        protected Collider m_Collider;

        protected NetworkVariable<bool> m_ResettingObject = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        protected IEnumerator checkOwnershipRoutine;

        Vector3 m_PrevPos;
        Vector3 m_SmoothVelocity;
        bool m_PauseVelocityCalculations = false;
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor m_CurrentInteractor;

        // ball scoring reference
        private BallScoring m_ball_scoring;

        // Lasso
        [SerializeField] private int interactorFramesToCalculate = 8;
        private Vector3[] interactorVelocityHistory;
        private int interactorFrameIndex = 0;
        private Vector3 interactorPrevPosition;
        private Vector3 averageHandVelocity;

        // Flag to track thrown/respawned for respawn
        public bool isThrown = false;
        public AssignPlayerColor.PlayerColor lastThrownPlayerColor = AssignPlayerColor.PlayerColor.None;

        [Header("Audio Options")]
        [SerializeField] private AudioSource m_ThrowLightAudioSource;
        [SerializeField] private AudioSource m_ThrowStrongAudioSource;
        [SerializeField] private AudioSource m_BounceSound;
        private AudioSource m_FlyingAudioSource;

        // Trail
        [Header("Trail Options")]
        [SerializeField] private GameObject trailPrefab;
        private List<TrailRenderer> m_TrailRenderers = new List<TrailRenderer>();


        public override void Awake()
        {
            base.Awake();

            m_Rigidbody = GetComponent<Rigidbody>();
            m_Collider = GetComponentInChildren<Collider>();
            m_ball_scoring = GetComponent<BallScoring>();
            m_FlyingAudioSource = GetComponent<AudioSource>();

            interactorVelocityHistory = new Vector3[interactorFramesToCalculate];

            if (trailPrefab != null)
            {
                // Adding multiple trails to the object
                for (int i = 0; i < 4; i++)
                {
                    GameObject trailInstance = Instantiate(trailPrefab, transform);
                    trailInstance.transform.localRotation = Quaternion.Euler(0, i * 90, 0);
                    m_TrailRenderers.AddRange(trailInstance.GetComponentsInChildren<TrailRenderer>());
                }

                deactivateTrailsRpc();
            }
            else
            {
                Debug.LogWarning("Trail Prefab is not assigned in the inspector.", this);
            }
        }

        [Rpc(SendTo.Everyone)]
        public void deactivateTrailsRpc()
        {
            foreach (var trail in m_TrailRenderers)
            {
                if (trail != null)
                {
                    trail.emitting = false;
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        public void triggerTrailsRpc(bool strongThrow)
        {   
            if (strongThrow) {
                // Enable/disable trails based on throw strength
                foreach (var trail in m_TrailRenderers)
                {
                    if (trail != null)
                    {
                        trail.emitting = true;
                    }
                } 
            }
            else {
                // TODO HACKY MACKY
                m_TrailRenderers[0].emitting = true;
            }
        }

        void FixedUpdate()
        {
            if (m_PauseVelocityCalculations) return;

            /*
            Vector3 velocity = (transform.position - m_PrevPos) / Time.fixedDeltaTime;

            float smoothingFactor = 0.1f; // Higher = more responsive, lower = smoother
            m_SmoothVelocity = Vector3.Lerp(m_SmoothVelocity, velocity, smoothingFactor);

            m_PrevPos = transform.position;*/


            // flying volume proportional to ball velocity
            m_FlyingAudioSource.volume = Mathf.Clamp(m_Rigidbody.linearVelocity.magnitude / 20.0f, 0.2f, 1.0f);
            m_BounceSound.volume = Mathf.Clamp(m_Rigidbody.linearVelocity.magnitude / 20.0f, 0.8f, 1.0f);
            m_BounceSound.pitch = 0.8f + Mathf.Clamp(m_Rigidbody.linearVelocity.magnitude / 80.0f, 0.0f, 0.5f);
            
            if (m_CurrentInteractor != null && isInteracting)
            {
                Vector3 currentInteractorPosition = m_CurrentInteractor.transform.position;
                Vector3 handVelocity = (currentInteractorPosition - interactorPrevPosition) / Time.fixedDeltaTime;

                interactorVelocityHistory[interactorFrameIndex] = handVelocity;
                interactorFrameIndex = (interactorFrameIndex + 1) % interactorFramesToCalculate;

                interactorPrevPosition = currentInteractorPosition;

                // Calculate average
                Vector3 total = Vector3.zero;
                for (int i = 0; i < interactorFramesToCalculate; i++)
                    total += interactorVelocityHistory[i];
                averageHandVelocity = total / interactorFramesToCalculate;

                // Update the lasso length based on the velocity
                UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor rayInteractor = m_CurrentInteractor as UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor;
                if (rayInteractor != null)
                {
                    float baseDistance = LassoCurve(averageHandVelocity.magnitude);
                    float newLassoDistance = Mathf.Clamp(baseDistance * 2f, 1.0f, 10.0f);

                    float smoothLassoDistance = Mathf.Lerp(rayInteractor.attachTransform.localPosition.z, newLassoDistance, Time.fixedDeltaTime * 0.75f);

                    // Set the attach point's local position along the Z-axis (forward)
                    rayInteractor.attachTransform.localPosition = new Vector3(0f, 0f, smoothLassoDistance);
                }
            }
        }

        float LassoCurve(float x)
        {
            if (x < 8f)
            {
                return x * 1.5f; // Linear growth
            }
            else
            {
                return 1f + Mathf.Log(x - 7f + 1f) * 0.5f; // Logarithmic rise after 8
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

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
            if (m_ResetObjectOnDisconnect && !m_Rigidbody.isKinematic)
            {
                ResetObjectPhysics();
            }
        }

        protected override void OnIsInteractingChanged(bool oldValue, bool newValue)
        {
            base.OnIsInteractingChanged(oldValue, newValue);
            if (IsOwner && newValue && m_LockedOnSpawn.Value)
            {
                m_LockedOnSpawn.Value = false;
            }

            if (!newValue)
            {
                m_Collider.enabled = false;
                m_Collider.enabled = true;
            }
        }

        void OnObjectPhysicsReset(bool oldValue, bool currentValue)
        {
            m_PauseVelocityCalculations = currentValue;
        }

        public override void ResetObject()
        {
            base.ResetObject();
            ResetObjectPhysics();
        }

        public void ResetObjectPhysics()
        {
            int originalInterpolation = (int)m_Rigidbody.interpolation;
            bool wasKinematic = m_Rigidbody.isKinematic;

            if (!wasKinematic)
            {
                m_Rigidbody.linearVelocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }

            m_Rigidbody.interpolation = RigidbodyInterpolation.None;
            m_Rigidbody.isKinematic = true;

            if (IsOwner && NetworkManager.IsConnectedClient && !NetworkManager.Singleton.ShutdownInProgress)
                m_ResettingObject.Value = true;

            StartCoroutine(ResetPhysicsRoutine(wasKinematic, originalInterpolation));
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

            // reset thrown
            isThrown = false;
            m_FlyingAudioSource.Stop();
            lastThrownPlayerColor = AssignPlayerColor.PlayerColor.None;

            // reset trails on grab
            deactivateTrailsRpc(); 

            // Reset Bounces on Ball Scoring
            m_ball_scoring.ResetBounces();
            // Reset Color on Ball Scoring
            m_ball_scoring.ResetColor();

            // Play haptics on both controllers on Item grab
            PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.BothController, 0.5f, 250, 50);

            if (m_IgnoreSocketSelectedCallback && args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
                return;

            // get the current interactor
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor interactor)
            {
                m_CurrentInteractor = interactor;
            }

            // reset velocities
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;

            // turn off gravity
            m_Rigidbody.useGravity = false;

            ClearHandVelocityHistory();
        }

        public override void OnSelectExitedLocal(BaseInteractionEventArgs args)
        {
            base.OnSelectExitedLocal(args);

            // Play haptics from audio file on both controllers on Item release
            AudioClip hapticAsset = Resources.Load<AudioClip>("sfx_blowingthrow_hapticversion");
            int sourceid = 0;
            PXR_Input.SendHapticBuffer(PXR_Input.VibrateType.BothController, hapticAsset, PXR_Input.ChannelFlip.No, ref sourceid);

            m_CurrentInteractor = null;

            if (m_IgnoreSocketSelectedCallback && args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
                return;

            if (m_BaseInteractable.isSelected) return;

            // set rigidbody velocity
            m_Rigidbody.linearVelocity = Mathf.Clamp(averageHandVelocity.magnitude * 20.0f, 0.025f, 80.0f) * averageHandVelocity.normalized;

            // turn on gravity
            m_Rigidbody.useGravity = true;

            // throw
            isThrown = true;
            m_FlyingAudioSource.Play();
            lastThrownPlayerColor = AssignPlayerColor.getPlayerColor();

            // play audio and trail
            bool strongThrow = m_Rigidbody.linearVelocity.magnitude > 75.0f;
            if (strongThrow) {
                m_Rigidbody.linearVelocity = m_Rigidbody.linearVelocity * 1.5f;
                m_ThrowStrongAudioSource.Play();
            }
            else {
                m_ThrowLightAudioSource.Play();
            }

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
            m_FlyingAudioSource.Play(); // ball is so far away we probably cant hear the sound anyways
            if (checkOwnershipRoutine != null) StopCoroutine(checkOwnershipRoutine);
            m_RequestingOwnership = false;
            CustomDebugLog.Singleton.LogNetworkManager($"Ownership Lost on Object {gameObject.name}");
        }

        void OnCollisionEnter(Collision collision)
        {

            if (collision.gameObject.CompareTag("Court"))
            {
                // play bounce sound
                m_BounceSound.Play();
            }

            if (!IsOwner || !m_AllowCollisionOwnershipExchange) return;

            NetworkPhysicsInteractable other = collision.transform.GetComponentInParent<NetworkPhysicsInteractable>();
            if (other != null && (isInteracting || IsMovingFaster(other.m_Rigidbody)))
            {
                other.RequestOwnership();
            }
        }

        bool IsMovingFaster(Rigidbody other)
        {
            return m_Rigidbody.linearVelocity.magnitude > m_MinExchangeVelocityMagnitude &&
                   m_Rigidbody.linearVelocity.magnitude > other.linearVelocity.magnitude;
        }

        public bool OwnershipTransferBlocked()
        {
            return isInteracting || IsOwner || m_RequestingOwnership || 
                   m_LockedOnSpawn.Value || !m_AllowCollisionOwnershipExchange || 
                   !NetworkObject.IsSpawned || baseInteractable.isSelected || m_ResettingObject.Value;
        }

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
            float waitTime = Mathf.Clamp(
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId) * 2f,
                0.025f, 5f
            );

            yield return new WaitForSeconds(waitTime);

            if (!IsOwner)
            {
                Debug.LogWarning($"Ownership Request Timed Out on Object {gameObject.name}");
            }

            m_RequestingOwnership = false;
        }

        void ClearHandVelocityHistory()
        {
            for (int i = 0; i < interactorFramesToCalculate; i++)
                interactorVelocityHistory[i] = Vector3.zero;

            interactorFrameIndex = 0;
            interactorPrevPosition = m_CurrentInteractor != null ? m_CurrentInteractor.transform.position : Vector3.zero;
            averageHandVelocity = Vector3.zero;
        }
    }
}
