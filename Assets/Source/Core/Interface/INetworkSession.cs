
namespace FrameLabs.Multiplayer.Core
{
    public interface INetworkSession
    {
        /// <summary>
        /// Starts the game as a host (server + local player).
        /// or run headless server
        /// </summary>
        void Host(bool headless = false);

        /// <summary>
        /// Joins an existing game as a client.
        /// </summary>
        void Join();

        /// <summary>
        /// Disconnects from the current session (client or host).
        /// </summary>
        void Leave();

        /// <summary>
        /// True if this instance is currently connected to a session.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// True if is a dedicated server
        /// </summary>
        bool IsDedicated { get; }

        /// <summary>
        /// True if this instance is the host.
        /// </summary>
        bool IsHost { get; }
    }
}