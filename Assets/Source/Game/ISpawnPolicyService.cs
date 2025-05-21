using UnityEngine;

namespace FrameLabs.Multiplayer.Game
{
    public interface ISpawnPolicyService<TRole>
    {
        GameObject SelectPrefab(ulong clientId);
        Vector3 SelectSpawnPoint(ulong clientId, int teamId);
        int AssignTeam(ulong clientId);
        TRole AssignRole(ulong clientId);
    }
}