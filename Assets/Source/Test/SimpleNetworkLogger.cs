using Unity.Netcode;
using UnityEngine;

public class SimpleNetworkLogger : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log($"[Simple] Network object spawned. IsHost={IsHost} IsOwner={IsOwner} ClientId={NetworkManager.Singleton.LocalClientId}");
    }
}
