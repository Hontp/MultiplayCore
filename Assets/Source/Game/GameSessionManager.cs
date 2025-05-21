using Unity.Netcode;
using UnityEngine;
using FrameLabs.Multiplayer.Core;

namespace FrameLabs.Multiplayer.Game
{
    public enum AgentRole
    {
        Default,
        Medic,
        Engineer,
    }

    /// <summary>
    /// Manages player join/leave and delegates spawning to PlayerLifecycleService.
    /// Not part of core networking—designed for gameplay coordination.
    /// </summary>
    public class GameSessionManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] spawnPrefabs;
        [SerializeField] private Transform[] spawnPoints;

        private PlayerLifecycleService<AgentRole> lifecycle;
        private ISpawnPolicyService<AgentRole> spawnPolicy;

        private void Start()
        {
            if( NetworkManager.Singleton == null )
            {
                Debug.LogError( "[GameSessionManager] NetworkManager.Singleton is null — aborting initialization." );
                return;
            }

            spawnPolicy = new DefaultSpawnPolicy<AgentRole>( spawnPrefabs, spawnPoints );
            lifecycle = new PlayerLifecycleService<AgentRole>(
                PlatformServices.NetworkEntityRegistry,
                spawnPolicy
            );

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            Debug.Log( "[GameSessionManager] Initialized successfully." );
        }

        private void HandleClientConnected( ulong clientId )
        {
            Debug.Log( $"[HOST] OnClientConnected: {clientId}" );

            if( NetworkManager.Singleton.IsServer )
            {
                lifecycle.SpawnPlayer( clientId );
                Debug.Log( $"[HOST] Spawned player for ClientId={clientId}" );
            }
        }

        private void HandleClientDisconnected( ulong clientId )
        {
            if( NetworkManager.Singleton.IsServer )
            {
                lifecycle.DespawnPlayer( clientId );
                Debug.Log( $"[GameSessionManager] Despawned player for ClientId={clientId}" );
            }
        }

        private void OnDestroy()
        {
            if( NetworkManager.Singleton != null )
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }
    }
}
