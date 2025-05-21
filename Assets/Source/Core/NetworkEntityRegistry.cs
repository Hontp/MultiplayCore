using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.Multiplayer.Core
{
    public class NetworkEntityRegistry : INetworkEntityRegistry
    {
        private readonly Dictionary<ulong, INetworkAgent> networkedPlayers = new();

        public void RegisterPlayer(ulong clientId, INetworkAgent agent)
        {
            if (!networkedPlayers.ContainsKey(clientId))
            {
                networkedPlayers.Add(clientId, agent);
                Debug.Log($"[NetworkEntityRegistry] Registered agent for ClientId {clientId}");
            }
        }

        public void UnregisterPlayer(ulong clientId)
        {
            if (networkedPlayers.Remove(clientId))
            {
                Debug.Log($"[NetworkEntityRegistry] Unregistered agent for ClientId {clientId}");
            }
        }

        public INetworkAgent GetAgentForClient(ulong clientId)
        {
            networkedPlayers.TryGetValue(clientId, out var agent);
            return agent;
        }

        public IReadOnlyDictionary<ulong, INetworkAgent> GetAll()
        {
            return networkedPlayers;
        }

        public IEnumerable<INetworkAgent> GetAllExcept(ulong excludedClientId)
        {
            foreach (var pair in networkedPlayers)
            {
                if (pair.Key != excludedClientId)
                    yield return pair.Value;
            }
        }

        public IEnumerable<INetworkAgent> GetAgentsNear(Vector3 position, float radius, Func<INetworkAgent, bool> filter = null)
        {
            float radiusSqr = radius * radius;

            foreach (var agent in networkedPlayers.Values)
            {
                if (agent is MonoBehaviour mb)
                {
                    if ((mb.transform.position - position).sqrMagnitude <= radiusSqr)
                    {
                        if (filter == null || filter(agent))
                            yield return agent;
                    }
                }
            }
        }

        public IEnumerable<INetworkAgent> Query(Func<ulong, INetworkAgent, bool> predicate)
        {
            foreach (var pair in networkedPlayers)
            {
                if (predicate(pair.Key, pair.Value))
                    yield return pair.Value;
            }
        }
    }
}