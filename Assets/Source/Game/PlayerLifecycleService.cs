using FrameLabs.Multiplayer.Core;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Multiplayer.Game
{
    /// <summary>
    /// Server-authoritative player cycle service for NGO.
    /// Uses a spawn policy to dynamically assign prefab, spawn position, team, and role.
    /// </summary>
    public class PlayerLifecycleService<TRole> : IPlayerLifecycleService
    {
        private readonly INetworkEntityRegistry playerRegistry;
        private readonly ISpawnPolicyService<TRole> spawnPolicy;

        public PlayerLifecycleService(INetworkEntityRegistry registry, ISpawnPolicyService<TRole> policy)
        {
            playerRegistry = registry;
            spawnPolicy = policy;
        }

        /// <summary>
        /// Spawns and registers a player object using the configured spawn policy.
        /// Server only.
        /// </summary>
        public void SpawnPlayer(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer) return;

            Debug.Log( $"[LIFECYCLE] Attempting to spawn player for clientId={clientId}" );

            // Resolve team, role, prefab, and position
            int teamId = spawnPolicy.AssignTeam(clientId);
            TRole role = spawnPolicy.AssignRole(clientId);
            GameObject prefab = spawnPolicy.SelectPrefab(clientId);
            Vector3 spawnPosition = spawnPolicy.SelectSpawnPoint(clientId, teamId);

            GameObject playerGO = GameObject.Instantiate(prefab, spawnPosition, Quaternion.identity);

            if (!playerGO.TryGetComponent(out NetworkObject networkObj))
            {
                Debug.LogError("[PlayerLifecycleService] Player prefab is missing NetworkObject.");
                GameObject.Destroy(playerGO);
                return;
            }

            Debug.Log( $"[PlayerLifecycleService] Spawning player for {clientId}" );

            // right before spawn:
            Debug.Log( $"Using prefab: {prefab.name}, at: {spawnPosition}" );

            networkObj.SpawnAsPlayerObject(clientId, destroyWithScene: true);

            Debug.Log( $"[LIFECYCLE] Spawned player prefab {prefab.name} for client {clientId}" );

            if (!playerGO.TryGetComponent(out INetworkAgent agent))
            {
                Debug.LogWarning("[PlayerLifecycleService] Spawned player does not implement INetworkAgent.");
                return;
            }

            // Set team and role via safe runtime interfaces
            if (playerGO.TryGetComponent(out ITeamAssignableSetter teamSetter))
                teamSetter.SetTeam(teamId);

            if (playerGO.TryGetComponent(out IAgentRoleSetter<TRole> roleSetter))
                roleSetter.SetRole(role);

            playerRegistry.RegisterPlayer(clientId, agent);
            agent.OnRegistered();
        }

        /// <summary>
        /// Despawns and unregisters the player for the given client ID.
        /// Server only.
        /// </summary>
        public void DespawnPlayer(ulong clientId)
        {
            var agent = playerRegistry.GetAgentForClient(clientId);
            if (agent != null)
            {
                agent.OnUnregistered();
                agent.Despawn();
                playerRegistry.UnregisterPlayer(clientId);
            }
        }

        /// <summary>
        /// Despawns and unregisters all currently tracked players.
        /// Server only.
        /// </summary>
        public void Reset()
        {
            // Despawn all agents
            foreach (var pair in playerRegistry.GetAll())
            {
                pair.Value?.OnUnregistered();
                pair.Value?.Despawn();
            }

            var clientIds = new List<ulong>();
            foreach (var kvp in playerRegistry.GetAll())
            {
                clientIds.Add(kvp.Key);
            }

            foreach (var clientId in clientIds)
            {
                playerRegistry.UnregisterPlayer(clientId);
            }

            clientIds.Clear();
        }

    }
}
