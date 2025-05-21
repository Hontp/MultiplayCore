
using FrameLabs.Multiplayer.Core;

namespace FrameLabs.Multiplayer.Game
{
    public interface IAgentInitializer
    {
        void AssignRole(INetworkAgent agent, ulong clientId);
        void AssignTeam(INetworkAgent agent, ulong clientId);
    }
}