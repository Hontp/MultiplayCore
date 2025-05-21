using FrameLabs.Multiplayer.Core;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace FrameLabs.Multiplayer.Game
{
    public static class AgentFilters
    {
        /// <summary>
        /// Returns a filter that excludes the specified client ID.
        /// </summary>
        public static Func<INetworkAgent, bool> ExcludeClient(ulong excludedClientId)
        {
            return agent =>
            {
                if (agent is NetworkBehaviour behaviour)
                    return behaviour.OwnerClientId != excludedClientId;
                return true;
            };
        }

        /// <summary>
        /// Returns a filter that includes only agents matching the specified tag.
        /// </summary>
        public static Func<INetworkAgent, bool> WithTag(string tag)
        {
            return agent =>
            {
                return (agent as MonoBehaviour)?.CompareTag(tag) == true;
            };
        }

        /// <summary>
        /// Returns a filter that includes only agents assigned to a specific team ID.
        /// Requires agent to implement ITeamAssignable.
        /// </summary>
        public static Func<INetworkAgent, bool> WithTeam(int teamId)
        {
            return agent =>
            {
                return (agent as ITeamAssignable)?.TeamId == teamId;
            };
        }

        /// <summary>
        /// Returns a filter that includes only agents not assigned to the given team.
        /// </summary>
        public static Func<INetworkAgent, bool> NotInTeam(int teamId)
        {
            return agent =>
            {
                return (agent as ITeamAssignable)?.TeamId != teamId;
            };
        }

        /// <summary>
        /// Returns a filter that includes only agents with a specific role.
        /// </summary>
        public static Func<INetworkAgent, bool> WithRole<TRole>(TRole requiredRole)
        {
            return agent =>
            {
                if (agent is IAgentRole<TRole> roleAgent)
                    return EqualityComparer<TRole>.Default.Equals(roleAgent.Role, requiredRole);
                return false;
            };
        }

        /// <summary>
        /// Returns a filter that includes only agents with roles different from the specified one.
        /// </summary>
        public static Func<INetworkAgent, bool> NotWithRole<TRole>(TRole excludedRole)
        {
            return agent =>
            {
                if (agent is IAgentRole<TRole> roleAgent)
                    return !EqualityComparer<TRole>.Default.Equals(roleAgent.Role, excludedRole);
                return true;
            };
        }
    }
}