using FrameLabs.Multiplayer.Core;

namespace FrameLabs.Multiplayer.Game
{
    public enum AgentRelationship
    {
        Neutral,
        Ally,
        Hostile,
        FriendlyNonCombatant
    }

    public interface IAgentClassification
    {
        /// <summary>
        /// Returns how this agent classifies the given target.
        /// The logic is up to the agent (e.g. AI, player, faction).
        /// </summary>
        AgentRelationship Classify(INetworkAgent target);
    }
}