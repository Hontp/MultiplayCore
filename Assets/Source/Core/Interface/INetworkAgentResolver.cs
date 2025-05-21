using UnityEngine;

namespace FrameLabs.Multiplayer.Core
{
    /// <summary>
    /// Resolves the INetworkAgent instance associated with a given GameObject.
    /// This abstraction decouples gameplay logic from specific network implementations
    /// </summary>
    public interface INetworkAgentResolver
    {
        /// <summary>
        /// Returns the network agent interface associated with a given GameObject.
        /// </summary>
        /// <param name="target">The GameObject to resolve from.</param>
        /// <returns>The INetworkAgent instance if one exists; otherwise, null.</returns>
        INetworkAgent GetAgentFor(GameObject target);
    }
}