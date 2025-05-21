using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;

namespace FrameLabs.Multiplayer.Core.UnityNet
{
#if UNITY_RELAY_SUPPORTED

    /// <summary>
    /// Unity Gaming Services implementation of IRelayTransportService using Unity Relay + UTP.
    /// Hosts relay sessions and enables NAT-punched client joining via join codes.
    /// </summary>

    public class UnityRelayTransportService : IRelayTransportService
    {
        private RelayTransportConfig config;
        private bool isConnected;

        private float lastActivityTime = -1f;
        private float inactivityTimeout = 60f;
        private bool transportFailureInactivity = false;

        public bool IsConnected => isConnected;

        public event Action<string> OnRelayShutdownReason;

        public void Configure(RelayTransportConfig config)
        {
            this.config = config;

            if (!string.Equals(config.Provider, "UnityRelay", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[UnityRelayTransportService] Unexpected provider: {config.Provider}");
            }

            if (config.RetryCount <= 0) config.RetryCount = 3;
            if (config.RetryDelayMs <= 0) config.RetryDelayMs = 250;

            if (config.Custom != null)
            {
                foreach (var kvp in config.Custom)
                    Debug.Log($"[UnityRelayTransportService] Custom config: {kvp.Key} = {kvp.Value}");
            }
        }

        public async Task<string> CreateRelayAsync(int maxConnections)
        {
            for (int attempt = 1; attempt <= config.RetryCount; attempt++)
            {
                try
                {
                    Debug.Log($"[Relay] Attempt {attempt} — requesting allocation...");

                    Allocation allocation = (config.Custom?.TryGetValue("region", out string region) == true)
                        ? await RelayService.Instance.CreateAllocationAsync(maxConnections, region)
                        : await RelayService.Instance.CreateAllocationAsync(maxConnections);

                    var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                    Debug.Log($"[Relay] Allocation ID: {allocation.AllocationId}");
                    Debug.Log($"[Relay] ConnectionData: {BitConverter.ToString(allocation.ConnectionData)}");
                    Debug.Log($"[Relay] Join code: {joinCode}");
                    Debug.Log($"[Relay] IsListening: {NetworkManager.Singleton.IsListening}");


                    if (string.IsNullOrEmpty(joinCode))
                    {
                        Debug.LogError("[Relay] Join code was null or empty!");
                        return null;
                    }

                    SetRelayTransportData(allocation);
                    isConnected = true;
                    InitializeInactivityWatch();

                    Debug.Log($"[Relay] Allocation successful. Join Code: {joinCode}");
                    GUIUtility.systemCopyBuffer =joinCode;

                    return joinCode;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Relay] Allocation failed on attempt {attempt}: {ex.Message}");
                    await Task.Delay(config.RetryDelayMs);
                }
            }

            Debug.LogError("[Relay] All allocation attempts failed. Aborting host setup.");
            isConnected = false;
            return null;
        }

        public async Task<bool> JoinRelayAsync(string joinCode)
        {
            try
            {
                Debug.Log($"[Client] Trying join code: {joinCode} at {Time.time:F2}");

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                string code = joinCode?.Trim().ToUpper();
                Debug.Log($"[Relay] Attempting to join with code: '{code}'");


                SetRelayTransportData(joinAllocation);
                isConnected = true;
                InitializeInactivityWatch();

                Debug.Log($"[Relay] Successfully joined relay session: {joinCode}");
                Debug.Log($"[Relay] Relay Join Success: {isConnected}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Relay] Failed to join relay with code '{joinCode}': {ex.Message}");

                if (joinCode.Length != 6)
                    Debug.LogWarning($"[Relay] Join code '{joinCode}' is unexpected length. Relay codes are 6 characters.");

                if (!joinCode.All(char.IsLetterOrDigit))
                    Debug.LogWarning($"[Relay] Join code '{joinCode}' contains invalid characters.");

                isConnected = false;
                return false;
            }
        }

        public void Shutdown()
        {
            isConnected = false;

            if (NetworkManager.Singleton?.NetworkConfig?.NetworkTransport is UnityTransport transport)
            {
                transport.Shutdown();
                Debug.Log("[Relay] Transport shutdown.");
            }
        }

        public void Tick()
        {
            if (NetworkManager.Singleton == null)
                return;

            if (!NetworkManager.Singleton.IsServer || !NetworkManager.Singleton.IsListening)
                return;

            if (NetworkManager.Singleton.ConnectedClients.Count > 1)
            {
                lastActivityTime = Time.time;
            }
            else if (lastActivityTime > 0 && Time.time - lastActivityTime > inactivityTimeout)
            {
                float idleTime = Time.time - lastActivityTime;
                Debug.LogWarning($"[Relay] Host shutting down due to inactivity timeout after {idleTime:F2} seconds of inactivity.");

                transportFailureInactivity = true;                
                OnRelayShutdownReason?.Invoke("Relay shut down due to inactivity.");

                NetworkManager.Singleton.Shutdown();
                lastActivityTime = -1f;
            }
        }

        public void InitializeInactivityWatch()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;

            lastActivityTime = Time.time;
        }

        private void OnTransportFailure()
        {
            if (transportFailureInactivity)
            {
                Debug.LogWarning($"[Relay] Inactivity timeout caused transport failure.");
            }
            else
            {
                Debug.LogError("[Relay] Transport failure occurred — likely real network error.");
                OnRelayShutdownReason?.Invoke("Relay transport failed due to network error.");
            }

            transportFailureInactivity = false;
            lastActivityTime = -1f;
        }

        public void ResetActivity()
        {
            lastActivityTime = Time.time;
        }

        public float TimeSinceLastActivity => (lastActivityTime > 0) ? Time.time - lastActivityTime : 0f;

        private void SetRelayTransportData(Allocation allocation)
        {
            if (NetworkManager.Singleton?.NetworkConfig?.NetworkTransport is not UnityTransport transport)
            {
                Debug.LogError("[Relay] UnityTransport is missing.");
                return;
            }

            if (allocation.RelayServer == null)
            {
                Debug.LogError("[Relay] Allocation.RelayServer is null. Cannot bind transport.");
                return;
            }

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                null,
                isSecure: true
            );

            Debug.Log("[Relay] RelayServerData bound (host).");
        }

        private void SetRelayTransportData(JoinAllocation allocation)
        {
            if (NetworkManager.Singleton?.NetworkConfig?.NetworkTransport is not UnityTransport transport)
            {
                Debug.LogError("[Relay] UnityTransport is missing.");
                return;
            }

            if (allocation.RelayServer == null)
            {
                Debug.LogError("[Relay] JoinAllocation.RelayServer is null. Cannot bind transport.");
                return;
            }

            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData,
                isSecure: true
            );

            Debug.Log("[Relay] RelayServerData bound (client).");
        }
    }
#endif
}
