using FrameLabs.Multiplayer.Core.UnityNet;
using System.Collections.Generic;


/// - INetworkSession - NetworkSession
/// - IRelayTransportService - UnityRelayTransportService
/// - IChatService - VivoxChatService
/// This class is intended to be injected into NetworkSessionManager and other high-level orchestrators.
namespace FrameLabs.Multiplayer.Core
{

    public static class RelayRegions
    {
        public const string US_East_1 = "us-east-1";
        public const string US_West_2 = "us-west-2";
        public const string SA_East_1 = "sa-east-1";
        public const string Europe_West_4 = "europe-west4";
        public const string Asia_Southeast_1 = "asia-southeast1";
        public const string Asia_East_1 = "asia-east1";
        public const string Australia_Southeast_1 = "australia-southeast1";
    }

    /// <summary>
    /// Factory that provides Unity Gaming Services-backed implementations of the core multiplayer interfaces.
    /// </summary>
    public class UnityNetworkServiceFactory : INetworkServiceFactory
    {
        public INetworkSession CreateNetworkSession() => new NetworkSession();

        public IRelayTransportService CreateRelayTransport()
        {
            var relay = new UnityRelayTransportService();
            relay.Configure(new RelayTransportConfig
            {
                Provider = "UnityRelay",
                RetryCount = 1,
                RetryDelayMs = 250,
                Custom = new Dictionary<string, string>
                {
                    { "region", RelayRegions.Australia_Southeast_1 }
                }
            });

            return relay;
        }

        public IChatService CreateChatService()
        {
            var chat = new VivoxChatService();
            chat.Configure(new VoiceChatConfig
            {
                Provider = "Vivox",
                Domain = "unity.vivox.com"
            });
            return chat;
        }
    }
}