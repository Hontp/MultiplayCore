using FrameLabs.Multiplayer.Core;

namespace FrameLabs.Multiplayer.Game
{
    public interface IRuleSet
    {
        bool CanDamage(INetworkAgent source, INetworkAgent target);
        bool IsObjectiveComplete();
        void OnAgentSpawned(INetworkAgent agent);
        void OnAgentDied(INetworkAgent agent);
    }
}