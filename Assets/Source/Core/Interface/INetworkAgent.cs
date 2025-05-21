using System;
using UnityEngine;


namespace FrameLabs.Multiplayer.Core
{
    public interface INetworkAgent
    {

        event Action<string> OnNetworkEventReceived;
        event Action<Vector3> OnTeleported;

        /// <summary>
        /// Teleports this agent to a new position.
        /// </summary>
        void Teleport(Vector3 position);

        /// <summary>
        /// Sends a named event across the network.
        /// Server relays the event to other clients.
        /// </summary>
        void SendEvent(string eventId);

        /// <summary>
        /// Destroys this agent from the network.
        /// Only the server may perform this action.
        /// </summary>
        void Despawn();

        /// <summary>
        /// Clears all event subscriptions attached to this agent.
        /// </summary>
        void CleanUpEvents();

        void OnRegistered();
        void OnUnregistered();

        ulong ClientId { get; }
    }
}