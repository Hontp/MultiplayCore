using System;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Core;

/// - Ensures UnityServices is initialized before UGS features are used
/// - Validates that UnityTransport is used when Relay is active
/// 
/// This validator should be injected into NetworkSessionManager.
namespace FrameLabs.Multiplayer.Core.UnityNet
{
    /// <summary>
    /// Validates Unity-specific runtime dependencies for multiplayer services.
    /// </summary>
    public class UnityEnvironmentValidator : INetworkEnvironmentValidator
    {
        public void ValidateGlobal()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.LogWarning("[UnityValidator] UnityServices not initialized. Call UnityServices.InitializeAsync() first.");
            }
        }

        public void ValidateForRelay()
        {
            if (NetworkManager.Singleton == null)
                throw new InvalidOperationException("[UnityValidator] NetworkManager.Singleton is missing from scene.");

            if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is not UnityTransport)
                throw new InvalidOperationException("[UnityValidator] UnityTransport is required for Unity Relay to function.");
        }
    }
}