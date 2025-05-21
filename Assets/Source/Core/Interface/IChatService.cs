using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace FrameLabs.Multiplayer.Core
{
    public struct VoiceChatConfig
    {
        public string Provider;        
        public string Domain;          
        public string AuthToken;       
        public string ServerUrl;       
        public Dictionary<string, string> Custom;
    }

    public interface IChatService
    {
        void Configure(VoiceChatConfig config);

        /// <summary>
        /// Initializes the voice system with this user's identity.
        /// </summary>
        void Initialize(string userId);

        /// <summary>
        /// Joins a voice channel by name.
        /// Can be global, team, or positional.
        /// </summary>
        void JoinChannel( string channelName, bool positional = false, GameObject trackedObject = null);

        /// <summary>
        /// Leaves the currently joined channel.
        /// </summary>
        void LeaveChannel(string channelName);

        /// <summary>
        /// Mutes or unmutes this user’s microphone.
        /// </summary>
        void SetMuted(bool muted);

        /// <summary>
        /// Updates the 3D position of the user (for positional voice).
        /// </summary>
        void Set3DPosition(Vector3 position);

        /// <summary>
        /// Send a text message, over the channel or direct to a user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channelOrUser"></param>
        /// <param name="direct"></param>
        void SendMessage(string message, string channelOrUser, bool direct);

        /// <summary>
        /// Event triggered when a text message is received.
        /// Parameters: senderId, message, channelName (null if direct message).
        /// </summary>
        event Action<string, string, string> OnTextMessageReceived;

        /// <summary>
        /// True if the user is currently connected to a voice session.
        /// </summary>
        bool IsConnected { get; }
    }

}
