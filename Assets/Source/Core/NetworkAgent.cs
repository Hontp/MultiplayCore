using FrameLabs.Multiplayer.Utilities;
using System;
using Unity.Netcode;
using UnityEngine;


namespace FrameLabs.Multiplayer.Core
{
    /// <summary>
    /// Concrete implementation of INetworkAgent using Unity Netcode.
    /// Responsible for teleporting the networked object in a server-authoritative way.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkAgent : NetworkBehaviour, INetworkAgent
    {
        public event Action<string> OnNetworkEventReceived;
        public event Action<Vector3> OnTeleported;

        private bool isRegistered;
        public ulong ClientId => NetworkObject.OwnerClientId;
        public bool IsRegistered => isRegistered;

        /// <summary>
        /// Automatically registers this agent on the server when spawned.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                PlatformServices.NetworkEntityRegistry?.RegisterPlayer(OwnerClientId, this);
                Debug.Log($"[NetworkAgent] Registered player agent for client {OwnerClientId}");
                Debug.Log( $"[PLAYER] OnNetworkSpawn: ClientId={NetworkManager.Singleton.LocalClientId}, IsOwner={IsOwner}" );
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                PlatformServices.NetworkEntityRegistry?.UnregisterPlayer(OwnerClientId);
                Debug.Log($"[NetworkAgent] Unregistered player agent for client {OwnerClientId}");
            }

            CleanUpEvents();
        }

        public virtual void OnRegistered()
        {
            isRegistered = true;
            Debug.Log($"[NetworkAgent] OnRegistered() called for client {OwnerClientId}");
        }

        public virtual void OnUnregistered()
        {
            isRegistered = false;
            Debug.Log($"[NetworkAgent] OnUnregistered() called for client {OwnerClientId}");
        }

        public void Teleport(Vector3 position)
        {
            if (IsServer)
            {
                ApplyTeleport(position);
                TeleportClientRpc(position);
            }
            else
            {
                RequestTeleportServerRpc(position);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestTeleportServerRpc(Vector3 position, ServerRpcParams rpcParams = default)
        {
            if (!NetworkSecurity.ValidateOwnership(NetworkObject, rpcParams))
                return;

            ApplyTeleport(position);
            TeleportClientRpc(position);
        }

        [ClientRpc]
        private void TeleportClientRpc(Vector3 position)
        {
            if (!IsServer)
            {
                ApplyTeleport(position);
                OnTeleported?.Invoke(position);
            }
        }

        public void SendEvent(string eventId)
        {
            if (IsServer)
            {
                EventClientRpc(eventId);
            }
            else
            {
                SendEventServerRpc(eventId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendEventServerRpc(string eventId, ServerRpcParams rpcParams = default)
        {
            if (!NetworkSecurity.ValidateOwnership(NetworkObject, rpcParams))
                return;

            EventClientRpc(eventId);
        }

        [ClientRpc]
        private void EventClientRpc(string eventId)
        {
            if (!IsServer)
            {
                OnNetworkEventReceived?.Invoke(eventId);
            }
        }

        public void Despawn()
        {
            if (IsServer)
            {
                NetworkObject.Despawn(true);
                Debug.Log($"[NetworkAgent] Despawned agent for client {OwnerClientId}");
            }
            else
            {
                Debug.LogWarning("[NetworkAgent] Only server can despawn agent.");
            }
        }

        public void CleanUpEvents()
        {
            OnNetworkEventReceived = null;
            OnTeleported = null;
        }

        private void ApplyTeleport(Vector3 position)
        {
            transform.position = position;
        }
    }
}