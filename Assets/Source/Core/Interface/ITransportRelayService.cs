using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameLabs.Multiplayer.Core
{
    public struct RelayTransportConfig
    {
        public string Provider;                      // e.g., "UnityRelay", "SteamRelay"
        public Dictionary<string, string> Custom;    // Extra options (e.g., region, QoS settings)
        public int RetryCount;                       // max allocation attempts
        public int RetryDelayMs;                     // delay (milliseconds) attmpts
    }

    public interface IRelayTransportService
    {
        /// <summary>
        /// Applies provider-specific configuration before startup.
        /// </summary>
        void Configure(RelayTransportConfig config);

        /// <summary>
        /// Starts and allocates a new relay session for others to join.
        /// Returns a unique join code (e.g., for client-side JoinGame).
        /// </summary>
        Task<string> CreateRelayAsync(int maxConnections);

        /// <summary>
        /// Joins an existing relay session using a join code.
        /// </summary>
        Task<bool> JoinRelayAsync(string joinCode);

        /// <summary>
        /// Track time since session start
        /// </summary>
        void Tick();

        /// <summary>
        /// Shuts down the relay transport session.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// True if the transport layer is connected and active.
        /// </summary>
        bool IsConnected { get; }
    }
}