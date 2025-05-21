using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace FrameLabs.Multiplayer.Core.UnityNet
{
    /// <summary>
    /// UnityTransport-based implementation of INetworkDiagnosticsService.
    /// Provides real-time RTT using Unity Transport (UTP) and simulates packet loss
    /// by sending pings to clients and tracking acknowledgments.
    /// </summary>
    public class UnityDiagnosticsService : INetworkDiagnosticsService
    {
        /// <summary>
        /// Interval between pings (in seconds).
        /// </summary>
        private const float PingInterval = 1.0f;

        private readonly Dictionary<ulong, PingState> clientPings = new();

        private float pingTimer = 0f;
        private int pingSequence = 0;

        /// <summary>
        /// Updates the diagnostics service.
        /// Should be called once per frame, typically from a central game loop or diagnostics manager.
        /// </summary>
        public void Update()
        {
            if (!IsSupported || !NetworkManager.Singleton.IsServer)
                return;

            pingTimer += Time.unscaledDeltaTime;
            if (pingTimer >= PingInterval)
            {
                pingTimer = 0f;
                BroadcastPing();
            }
        }

        /// <summary>
        /// Broadcasts a ping to all connected clients using a ping sequence number.
        /// Expects a return via ServerRpc for packet loss simulation.
        /// </summary>
        private void BroadcastPing()
        {
            pingSequence++;

            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!clientPings.TryGetValue(clientId, out var state))
                {
                    state = new PingState();
                    clientPings[clientId] = state;
                }

                state.Expected++;
                SendPingClientRpc(pingSequence, clientId);
            }
        }

        /// <summary>
        /// ClientRpc sent from server to client. Carries a ping sequence number.
        /// The intended client responds with a ServerRpc to simulate ACK receipt.
        /// </summary>
        [ClientRpc]
        private void SendPingClientRpc(int seq, ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                ReturnPongServerRpc(seq);
            }
        }

        /// <summary>
        /// ServerRpc sent back by client in response to a ping. Increments the received counter.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void ReturnPongServerRpc(int seq, ServerRpcParams rpcParams = default)
        {
            if (clientPings.TryGetValue(rpcParams.Receive.SenderClientId, out var state))
            {
                state.Received++;
            }
        }

        /// <summary>
        /// Gets the current round-trip time (RTT) in milliseconds for the specified client.
        /// Relies on Unity Transport’s built-in RTT support.
        /// </summary>
        public float GetRTT(ulong clientId)
        {
            if (NetworkManager.Singleton == null ||
                !NetworkManager.Singleton.IsListening ||
                NetworkManager.Singleton.NetworkConfig.NetworkTransport is not UnityTransport utp)
                return -1f;

            var client = NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)
                ? NetworkManager.Singleton.ConnectedClients[clientId]
                : null;

            if (client == null || client.ClientId != clientId)
                return -1f;

            try
            {
                return utp.GetCurrentRtt(clientId);
            }
            catch
            {
                // Connection may not be ready in UTP internal map
                return -1f;
            }
        }


        /// <summary>
        /// Returns the estimated packet loss percentage for a given client (0–100).
        /// Simulated via ping/ack counters tracked on the server.
        /// </summary>
        public float GetPacketLoss(ulong clientId)
        {
            if (clientPings.TryGetValue(clientId, out var state) && state.Expected > 0)
            {
                float lost = state.Expected - state.Received;
                return Mathf.Clamp01(lost / state.Expected) * 100f;
            }

            return 0f;
        }

        /// <summary>
        /// Returns the name of the active transport implementation (e.g. UnityTransport, CustomTransport).
        /// </summary>
        public string GetTransportName()
        {
            return NetworkManager.Singleton.NetworkConfig.NetworkTransport?.GetType().Name ?? "Unknown";
        }

        /// <summary>
        /// Returns true if the diagnostics system is currently functional.
        /// </summary>
        public bool IsSupported =>
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening &&
            NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport;

        /// <summary>
        /// Indicates RTT measurement is available (always true for UnityTransport).
        /// </summary>
        public bool SupportsRTT => true;

        /// <summary>
        /// Indicates packet loss simulation is available via ping/ack tracking.
        /// </summary>
        public bool SupportsPacketLoss => true;

        /// <summary>
        /// Tracks the ping state (expected vs. received) for each client.
        /// Used to estimate packet loss.
        /// </summary>
        private class PingState
        {
            public int Expected;
            public int Received;
        }
    }
}
