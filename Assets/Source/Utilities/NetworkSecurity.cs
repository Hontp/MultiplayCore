using Unity.Netcode;
using UnityEngine;

namespace FrameLabs.Multiplayer.Utilities
{
    public static class NetworkSecurity
    {
        /// <summary>
        /// Validates whether the client who sent the RPC actually owns the given NetworkObject.
        /// </summary>
        /// <param name="networkObject">The NetworkObject being acted upon.</param>
        /// <param name="rpcParams">The ServerRpcParams passed into the RPC.</param>
        /// <returns>True if the sender is the owner; false otherwise.</returns>
        public static bool ValidateOwnership(NetworkObject networkObject, ServerRpcParams rpcParams)
        {
            if (networkObject == null)
            {
                Debug.LogWarning("[RpcSecurity] NetworkObject is null.");
                return false;
            }

            if (rpcParams.Receive.SenderClientId != networkObject.OwnerClientId)
            {
                Debug.LogWarning($"[RpcSecurity] Client {rpcParams.Receive.SenderClientId} attempted to act on object owned by {networkObject.OwnerClientId}. Rejected.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates whether the client who sent the RPC owns the given NetworkObject.
        /// </summary>
        public static bool IsOwner(NetworkObject networkObject, ServerRpcParams rpcParams)
        {
            if (networkObject == null)
            {
                Debug.LogWarning("[RpcValidation] NetworkObject is null.");
                return false;
            }

            if (rpcParams.Receive.SenderClientId != networkObject.OwnerClientId)
            {
                Debug.LogWarning($"[RpcValidation] Client {rpcParams.Receive.SenderClientId} tried to modify object owned by {networkObject.OwnerClientId}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that only the host (server with ClientId 0) is allowed to invoke this RPC.
        /// </summary>
        public static bool IsHost(ServerRpcParams rpcParams)
        {
            if (rpcParams.Receive.SenderClientId != 0)
            {
                Debug.LogWarning($"[RpcValidation] Only host (ClientId 0) may perform this action. Sender was {rpcParams.Receive.SenderClientId}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates whether the caller is in the allowed client ID list.
        /// </summary>
        public static bool IsAuthorizedClient(ServerRpcParams rpcParams, ulong[] allowedClients)
        {
            ulong sender = rpcParams.Receive.SenderClientId;

            foreach (ulong clientId in allowedClients)
            {
                if (clientId == sender)
                    return true;
            }

            Debug.LogWarning($"[RpcValidation] Client {sender} is not in the authorized client list.");
            return false;
        }
    }
}