using FrameLabs.Multiplayer.Core;
using UnityEngine;

namespace FrameLabs.Multiplayer.Core
{
    /// <summary>
    /// Resolves INetworkAgent instances from GameObjects by retrieving NetworkAgent components.
    /// This binds NGO to your abstraction layer without exposing it to gameplay logic.
    /// </summary>
    public class NetworkAgentResolver : INetworkAgentResolver
    {
        public INetworkAgent GetAgentFor(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("NetworkAgentResolver: Provided GameObject is null.");
                return null;
            }

            var agent = target.GetComponent<INetworkAgent>();
            if (agent == null)
            {
                Debug.LogWarning($"NetworkAgentResolver: No INetworkAgent found on GameObject '{target.name}'.");
            }

            return agent;
        }
    }
}