using UnityEngine;


namespace FrameLabs.Multiplayer.Game
{
    public interface IPlayerLifecycleService
    {
        /// <summary>
        /// Manually spawns a remote player for the given clientId.
        /// Should only be called on the server.
        /// </summary>
        void SpawnPlayer(ulong clientId);

        /// <summary>
        /// Despawns and unregisters a player when they disconnect.
        /// </summary>
        void DespawnPlayer(ulong clientId);

        /// <summary>
        /// Clears all active player references.
        /// </summary>
        void Reset();
 
    }
}