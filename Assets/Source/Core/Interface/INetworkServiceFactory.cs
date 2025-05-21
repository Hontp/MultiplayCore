namespace FrameLabs.Multiplayer.Core
{
    public interface INetworkServiceFactory
    {
        INetworkSession CreateNetworkSession();
        IRelayTransportService CreateRelayTransport();
        IChatService CreateChatService();
    }
}
