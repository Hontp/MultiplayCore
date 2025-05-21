using System;
using UnityEngine;


namespace FrameLabs.Multiplayer.Core
{
    public interface INetworkClockService
    {
        float Uptime {  get; }
        float TickInterval {  get; }
        int TickCount {  get; }

        event Action<int, float> OnTick;
 
        void StartClock();
        void Stop();
    }
}