using System.Threading.Tasks;
using UnityEngine;

namespace FrameLabs.Multiplayer.Core
{
    /// <summary>
    /// Composite session that layers relay transport on top of a base INetworkSession implementation.
    /// Delegates all lifecycle to the inner session while optionally managing relay allocations.
    /// </summary>
    public class RelayTransportSession : INetworkSession
    {
        private readonly INetworkSession baseSession;
        private readonly IRelayTransportService relay;
        private bool relayUsed = false;

        public RelayTransportSession(INetworkSession baseSession, IRelayTransportService relay)
        {
            this.baseSession = baseSession;
            this.relay = relay;
        }

        ///<inheritdoc/>
        public bool IsConnected => baseSession.IsConnected;

        ///<inheritdoc/>
        public bool IsHost => baseSession.IsHost;

        ///<inheritdoc/>
        public bool IsDedicated => baseSession.IsDedicated;

        /// <summary>
        /// Creates a relay allocation and starts hosting as host or dedicated server.
        /// Returns the generated join code (or null on failure).
        /// </summary>
        public async Task<string> HostWithRelayAsync(int maxConnections, bool dedicated = false)
        {
            string joinCode = await relay.CreateRelayAsync(maxConnections);
            if (!string.IsNullOrEmpty(joinCode))
            {
                relayUsed = true;
                baseSession.Host(dedicated);
                return joinCode;
            }

            Debug.LogError("[RelayTransportSession] Failed to host with relay.");
            return null;
        }

        /// <summary>
        /// Joins a relay session using a join code, then starts as client.
        /// </summary>
        public async Task<bool> JoinWithRelayAsync(string joinCode)
        {
            bool success = await relay.JoinRelayAsync(joinCode);
            if (success)
            {
                relayUsed = true;
                baseSession.Join();
                return true;
            }

            Debug.LogError("[RelayTransportSession] Failed to join with relay.");
            return false;
        }

        /// <summary>
        /// Starts a host session without relay (peer-host or dedicated).
        /// Will warn if relay has already been used.
        /// </summary>
        public void Host(bool dedicated = false)
        {
            if (relayUsed)
            {
                Debug.LogWarning("[RelayTransportSession] Relay was already used. Use HostWithRelayAsync() instead.");
                return;
            }

            baseSession.Host(dedicated);
        }

        /// <summary>
        /// Starts a client session without relay (e.g. direct IP).
        /// </summary>
        public void Join()
        {
            if (relayUsed)
            {
                Debug.LogWarning("[RelayTransportSession] Relay was already used. Use JoinWithRelayAsync() instead.");
                return;
            }

            baseSession.Join();
        }

        /// <summary>
        /// Disconnects the session and shuts down relay transport if in use.
        /// </summary>
        public void Leave()
        {
            baseSession.Leave();

            if (relayUsed)
            {
                relay.Shutdown();
                relayUsed = false;
            }
        }
    }
}
