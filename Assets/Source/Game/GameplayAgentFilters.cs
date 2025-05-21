using FrameLabs.Multiplayer.Core;
using System;
using System.Collections.Generic;

namespace FrameLabs.Multiplayer.Game
{
    /// <summary>
    /// Provides reusable gameplay-level filters for classifying and querying INetworkAgents
    /// based on agent relationship semantics such as ally, hostile, neutral, etc.
    /// </summary>
    public static class GameplayAgentFilters
    {
        /// <summary>
        /// Returns a filter that includes agents the source classifies as the given relationship type.
        /// </summary>
        public static Func<INetworkAgent, bool> WhereClassifiedAs(
            IAgentClassification source,
            AgentRelationship relationship)
        {
            return agent => source.Classify(agent) == relationship;
        }

        /// <summary>
        /// Returns a filter that includes only agents not classified as the given type.
        /// </summary>
        public static Func<INetworkAgent, bool> WhereNotClassifiedAs(
            IAgentClassification source,
            AgentRelationship excluded)
        {
            return agent => source.Classify(agent) != excluded;
        }

        /// <summary>
        /// Returns a filter that includes agents classified as ANY of the specified types.
        /// </summary>
        public static Func<INetworkAgent, bool> ClassifiedAsAny(
            IAgentClassification source,
            params AgentRelationship[] types)
        {
            return agent =>
            {
                var rel = source.Classify(agent);
                for (int i = 0; i < types.Length; i++)
                {
                    if (rel == types[i])
                        return true;
                }
                return false;
            };
        }

        /// <summary>
        /// Returns a filter that excludes agents classified as any of the specified types.
        /// </summary>
        public static Func<INetworkAgent, bool> WhereNotClassifiedIn(
            IAgentClassification source,
            params AgentRelationship[] types)
        {
            return agent =>
            {
                var rel = source.Classify(agent);
                for (int i = 0; i < types.Length; i++)
                {
                    if (rel == types[i])
                        return false;
                }
                return true;
            };
        }

        /// <summary>
        /// Returns a fallback-safe filter that includes agents the source classifies as the expected type.
        /// </summary>
        public static Func<INetworkAgent, bool> SafeWhereClassifiedAs(IAgentClassification source,AgentRelationship expected)
        {
            return agent => source != null && source.Classify(agent) == expected;
        }

        /// <summary>
        /// Filters for hostile agents only.
        /// </summary>
        public static Func<INetworkAgent, bool> HostilesOnly(IAgentClassification source)
            => WhereClassifiedAs(source, AgentRelationship.Hostile);

        /// <summary>
        /// Filters for allies only.
        /// </summary>
        public static Func<INetworkAgent, bool> AlliesOnly(IAgentClassification source)
            => WhereClassifiedAs(source, AgentRelationship.Ally);

        /// <summary>
        /// Filters for neutral agents only.
        /// </summary>
        public static Func<INetworkAgent, bool> NeutralsOnly(IAgentClassification source)
            => WhereClassifiedAs(source, AgentRelationship.Neutral);

        /// <summary>
        /// Filters for non-combatant friendly agents only.
        /// </summary>
        public static Func<INetworkAgent, bool> FriendliesOnly(IAgentClassification source)
            => WhereClassifiedAs(source, AgentRelationship.FriendlyNonCombatant);

        /// <summary>
        /// Returns all agents classified as Hostile by the source, using an internal cache to avoid redundant calls.
        /// </summary>
        public static IEnumerable<INetworkAgent> CachedHostilesNear(
            IAgentClassification source,
            IEnumerable<INetworkAgent> all)
        {
            Dictionary<INetworkAgent, AgentRelationship> cache = new();

            foreach (var agent in all)
            {
                if (!cache.TryGetValue(agent, out var rel))
                {
                    rel = source.Classify(agent);
                    cache[agent] = rel;
                }

                if (rel == AgentRelationship.Hostile)
                    yield return agent;
            }
        }
    }
}
