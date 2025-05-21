using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.Multiplayer.Core
{
    public interface INetworkEntityRegistry
    {
        /// <summary>
        /// Registers an agent with the registry for a specific client.
        /// </summary>
        void RegisterPlayer(ulong clientId, INetworkAgent agent);

        /// <summary>
        /// Unregisters an agent associated with the client.
        /// </summary>
        void UnregisterPlayer(ulong clientId);

        /// <summary>
        /// Gets the agent associated with a specific client ID.
        /// </summary>
        INetworkAgent GetAgentForClient(ulong clientId);

        /// <summary>
        /// Gets all currently registered player agents.
        /// </summary>
        IReadOnlyDictionary<ulong, INetworkAgent> GetAll();

        IEnumerable<INetworkAgent> GetAllExcept(ulong excludedClientId);
        IEnumerable<INetworkAgent> GetAgentsNear(Vector3 position, float radius, Func<INetworkAgent, bool> filter = null);
        IEnumerable<INetworkAgent> Query(Func<ulong, INetworkAgent, bool> predicate);
    }
}