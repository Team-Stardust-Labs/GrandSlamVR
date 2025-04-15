using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace XRMultiplayer
{
    [RequireComponent(typeof(Rigidbody))]
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
        Vector3 m_AverageVelocity;
        bool m_PauseVelocityCalculations = false;

        public override void Awake()
        {
            base.Awake();

            m_Rigidbody = GetComponent<Rigidbody>();
            m_Collider = GetComponentInChildren<Collider>();

        }

        void FixedUpdate()
        {
            if (m_PauseVelocityCalculations) return;

            Vector3 velocity = (transform.position - m_PrevPos) / Time.fixedDeltaTime;

            float smoothingFactor = 0.25f; // Higher = more responsive, lower = smoother
            m_AverageVelocity = Vector3.Lerp(m_AverageVelocity, velocity, smoothingFactor);

            m_PrevPos = transform.position;
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
                m_Rigidbody.velocity = Vector3.zero;
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

            if (m_IgnoreSocketSelectedCallback && args.interactorObject is XRSocketInteractor)
                return;
        }

        public override void OnSelectExitedLocal(BaseInteractionEventArgs args)
        {
            base.OnSelectExitedLocal(args);

            if (m_IgnoreSocketSelectedCallback && args.interactorObject is XRSocketInteractor)
                return;

            if (m_BaseInteractable.isSelected) return;


            if (IsOwner && baseInteractable is XRGrabInteractable grab &&
                (grab.movementType == XRBaseInteractable.MovementType.VelocityTracking || grab.throwOnDetach))
            {
                m_Rigidbody.isKinematic = false;
                //m_Rigidbody.velocity = m_AverageVelocity.normalized * Mathf.Sqrt(m_AverageVelocity.magnitude);
                m_Rigidbody.velocity = m_AverageVelocity.normalized * Mathf.Clamp(m_AverageVelocity.magnitude, 0.025f, 31.415f);
            }
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
            if (checkOwnershipRoutine != null) StopCoroutine(checkOwnershipRoutine);
            m_RequestingOwnership = false;
            CustomDebugLog.Singleton.LogNetworkManager($"Ownership Lost on Object {gameObject.name}");
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!IsOwner || !m_AllowCollisionOwnershipExchange) return;

            NetworkPhysicsInteractable other = collision.transform.GetComponentInParent<NetworkPhysicsInteractable>();
            if (other != null && (isInteracting || IsMovingFaster(other.m_Rigidbody)))
            {
                other.RequestOwnership();
            }
        }

        bool IsMovingFaster(Rigidbody other)
        {
            return m_Rigidbody.velocity.magnitude > m_MinExchangeVelocityMagnitude &&
                   m_Rigidbody.velocity.magnitude > other.velocity.magnitude;
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


    }
}
