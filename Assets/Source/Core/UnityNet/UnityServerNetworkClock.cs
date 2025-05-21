using FrameLabs.Multiplayer.Core;
using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class UnityServerNetworkClock : MonoBehaviour, INetworkClockService
{
    [SerializeField] private float tickInterval = 1f;

    public float TickInterval => tickInterval;
    public float Uptime { get; private set; } = 0f;
    public int TickCount { get; private set; } = 0;

    public event Action<int, float> OnTick;

    private float timer = 0f;
    private bool running = false;

    public static INetworkClockService Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        }
        else
        {
            Debug.LogWarning("[ServerClock] NetworkManager not available during Awake.");
        }
    }

    private void HandleServerStarted()
    {
        Debug.Log("[ServerClock] Server started — ticking clock.");
        StartClock();
    }

    public void StartClock()
    {
        running = true;
        Uptime = 0f;
        TickCount = 0;
        timer = 0f;
    }

    public void Stop()
    {
        running = false;
    }

    private void Update()
    {
        if (!running || !NetworkManager.Singleton.IsServer)
            return;

        Uptime += Time.deltaTime;
        timer += Time.deltaTime;

        if (timer >= tickInterval)
        {
            timer -= tickInterval;
            TickCount++;

            OnTick?.Invoke(TickCount, Uptime);

            KeepRelayAlive();
        }
    }

    [ClientRpc]
    private void KeepRelayAlive() { }
}
