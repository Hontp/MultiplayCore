using FrameLabs.Multiplayer.Game;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;


/// This class is backend-agnostic and operates solely through abstracted service interfaces:
/// - INetworkSession: encapsulates host/client connection logic
/// - IRelayTransportService: manages relay join/host operations (e.g., Unity Relay)
/// - IChatService: optional positional and direct voice/text communication
/// 
/// Responsibilities:
/// - Hosts or joins a network session via injected factory
/// - Optionally enables voice chat and relay transport
/// - Ensures runtime dependencies are validated before usage
/// - Provides fallback-safe TryHost/TryJoin variants with error logging

namespace FrameLabs.Multiplayer.Core
{
    public struct NetworkSessionConfig
    {
        public bool UseRelay;
        public int MaxConnections;
        public string RelayProvider;

        public bool UseVoice;
        public string VoiceProvider;
        public string UserId;

        public bool IsDedicated;
    }

    /// <summary>
    /// Setup multiplayer session lifecycle including hosting, joining, voice setup.
    /// </summary>
    public class NetworkSessionManager
    {
        private readonly INetworkServiceFactory serviceFactory;
        private readonly INetworkEnvironmentValidator environmentValidator;
        private IRelayTransportService relayTransportService;

        public INetworkSession Session { get; private set; }
        public IChatService Voice { get; private set; }
        public string JoinCode { get; private set; }

        public NetworkSessionManager(INetworkServiceFactory factory, INetworkEnvironmentValidator validator)
        {
            serviceFactory = factory;
            environmentValidator = validator;
        }

        public void PreInitializationCheck()
        {
            environmentValidator.ValidateGlobal();
        }

        public async Task InitializeHostAsync(NetworkSessionConfig config)
        {
            var baseSession = serviceFactory.CreateNetworkSession();

            if (config.UseRelay)
            {
                relayTransportService = serviceFactory.CreateRelayTransport();
                Session = new RelayTransportSession(baseSession, relayTransportService);

                environmentValidator.ValidateForRelay();
                JoinCode = await ((RelayTransportSession)Session).HostWithRelayAsync(
                    config.MaxConnections,
                    config.IsDedicated
                );

                if (string.IsNullOrEmpty(JoinCode))
                    throw new Exception("[SessionManager] Relay allocation failed.");
            }
            else
            {
                Session = baseSession;
                Session.Host(config.IsDedicated);
            }

            if (config.UseVoice)
            {
                Voice = serviceFactory.CreateChatService();
                Voice.Initialize(config.UserId);
            }          
        }

        public async Task<bool> TryInitializeHostAsync(NetworkSessionConfig config)
        {
            try
            {
                await InitializeHostAsync(config);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SessionManager] Failed to host session: {ex.Message}");
                return false;
            }
        }

        public void Update()
        {
            if (relayTransportService == null)
                return;

            if (relayTransportService.IsConnected)
                relayTransportService.Tick();
        }


        public async Task<bool> JoinAsync(string joinCode, NetworkSessionConfig config)
        {
            var baseSession = serviceFactory.CreateNetworkSession();

            if (config.UseRelay)
            {
                var relay = serviceFactory.CreateRelayTransport();
                Session = new RelayTransportSession(baseSession, relay);

                environmentValidator.ValidateForRelay();

                if (!await ((RelayTransportSession)Session).JoinWithRelayAsync(joinCode))
                    return false;
            }
            else
            {
                Session = baseSession;
                Session.Join();
            }

            if (config.UseVoice)
            {
                Voice = serviceFactory.CreateChatService();
                Voice.Initialize(config.UserId);
            }

            return true;
        }

        public async Task<bool> TryJoinAsync(string joinCode, NetworkSessionConfig config)
        {
            try
            {
                return await JoinAsync(joinCode, config);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SessionManager] Failed to join session: {ex.Message}");
                return false;
            }
        }

        public void Leave()
        {
            Session?.Leave();
            Voice?.SetMuted(true);
        }
    }

}
