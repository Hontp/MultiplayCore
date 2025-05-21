using Unity.Netcode;
using UnityEngine;

namespace FrameLabs.Multiplayer.Core
{
    public class NetworkSession : INetworkSession
    {
        private bool isDedicated = false;

        public bool IsConnected => NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer;
        public bool IsHost => NetworkManager.Singleton.IsHost;

        public bool IsDedicated => isDedicated;

        public void Host(bool dedicated)
        {
            isDedicated = dedicated;

            if (isDedicated)
            {
                NetworkManager.Singleton.StartServer();
                Debug.Log("[NetworkSession] Starting server...");
            }
            else
            {
                NetworkManager.Singleton.StartHost();
                Debug.Log("[NetworkSession] Hosting game...");
            }
        }

        public void Join()
        {
            bool success = NetworkManager.Singleton.StartClient();
            Debug.Log("[NetworkSession] Joining game...");

            if (!success) 
            {
                Debug.Log( "[NetworkSession] Failed to join game..." );
            }
        }

        public void Leave()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                Debug.Log("[NetworkSession] Stopping host/server...");
                NetworkManager.Singleton.Shutdown();
                isDedicated = false;
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("[NetworkSession] Disconnecting client...");
                NetworkManager.Singleton.Shutdown();
            }
        }

    }
}