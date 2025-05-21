using System.Threading.Tasks;
using UnityEngine;

namespace FrameLabs.Multiplayer.Core
{
    /// <summary>
    /// Static service locator for accessing platform-agnostic multiplayer features.
    /// This provides backend-resolved interfaces to gameplay code.
    /// </summary>
    public static class PlatformServices
    {
        private static INetworkAgentResolver netAgent;
        private static INetworkEntityRegistry networkEntityRegistry;
        private static NetworkSessionManager sessionManager;

        /// <summary>
        /// Gets the currently active INetworkSession (e.g., host/client state).
        /// Delegates to NetworkSessionManager.Session.
        /// </summary>
        public static INetworkSession Session => sessionManager?.Session;

        /// <summary>
        /// Gets the networked agent registry used for runtime agent lookups.
        /// </summary>
        public static INetworkEntityRegistry NetworkEntityRegistry => networkEntityRegistry;

        /// <summary>
        /// Returns the active diagnostics service. Guaranteed to be non-null.
        /// Defaults to a no-op implementation if not explicitly registered.
        /// </summary>
        public static INetworkDiagnosticsService Diagnostics { get; private set; } = new NoDiagnosticsService();

        /// <summary>
        /// Gets the underlying session manager.
        /// Use this if you need access to JoinCode, Voice, or full session control.
        /// </summary>
        public static NetworkSessionManager SessionManager => sessionManager;

        /// <summary>
        /// Registers the full multiplayer stack for the current platform implementation.
        /// This includes agent resolution, session management, registry, and optional diagnostics.
        /// </summary>
        public static void Register(
            INetworkAgentResolver agentResolver,
            NetworkSessionManager manager,
            INetworkEntityRegistry registry,
            INetworkDiagnosticsService diagnosticsService = null)
        {
            sessionManager = manager;
            netAgent = agentResolver;
            networkEntityRegistry = registry;
            Diagnostics = diagnosticsService ?? new NoDiagnosticsService();
        }

        /// <summary>
        /// Attempts to initialize a host session using the configured platform services.
        /// Handles relay, voice, and transport as configured.
        /// </summary>
        public static Task<bool> TryInitializeHostAsync(NetworkSessionConfig config)
        {
            if (sessionManager == null)
            {
                Debug.LogError("[PlatformServices] SessionManager not registered.");
                return Task.FromResult(false);
            }

            return sessionManager.TryInitializeHostAsync(config);
        }

        /// <summary>
        /// Attempts to join a remote session using a relay join code.
        /// </summary>
        public static Task<bool> TryJoinAsync(string joinCode, NetworkSessionConfig config)
        {
            if (sessionManager == null)
            {
                Debug.LogError("[PlatformServices] SessionManager not registered.");
                return Task.FromResult(false);
            }

            return sessionManager.TryJoinAsync(joinCode, config);
        }

        /// <summary>
        /// Retrieves the INetworkAgent for the given GameObject.
        /// Returns null if not found or not registered.
        /// </summary>
        public static INetworkAgent GetAgentFor(GameObject target)
        {
            if (netAgent == null)
            {
                Debug.LogError("[PlatformServices] INetworkAgentResolver not registered.");
                return null;
            }

            return netAgent.GetAgentFor(target);
        }

        /// <summary>
        /// Updates the network diagnostics system, if available.
        /// Should be called once per frame.
        /// </summary>
        public static void RunDiagnostics()
        {
            Diagnostics.Update();
        }

        /// <summary>
        /// Returns true if the platform services have been initialized.
        /// </summary>
        public static bool IsInitialized => netAgent != null && sessionManager != null && networkEntityRegistry != null;
    }
}