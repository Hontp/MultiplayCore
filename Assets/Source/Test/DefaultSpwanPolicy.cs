using FrameLabs.Multiplayer.Game;
using UnityEngine;

public class DefaultSpawnPolicy<TRole> : ISpawnPolicyService<TRole>
{
    private readonly GameObject[] prefabs;
    private readonly Transform[] spawnPoints;

    public DefaultSpawnPolicy( GameObject[] prefabs, Transform[] points )
    {
        this.prefabs = prefabs;
        this.spawnPoints = points;
    }

    public GameObject SelectPrefab( ulong clientId ) => prefabs[ 0 ];
    public Vector3 SelectSpawnPoint( ulong clientId, int teamId ) => spawnPoints[ clientId % (ulong)spawnPoints.Length ].position;
    public int AssignTeam( ulong clientId ) => (int)( clientId % 2 ); // Example: even/odd teams
    public TRole AssignRole( ulong clientId ) => default;
}
