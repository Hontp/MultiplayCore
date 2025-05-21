using UnityEngine;
using System;
using System.Threading.Tasks;

namespace FrameLabs.Multiplayer.Core.UnityNet
{
    
#if VIVOX_SUPPORTED

    using Unity.Services.Vivox;

    /// <summary>
    /// Unity Vivox implementation of IChatService.
    /// Provides positional audio, channel join/leave, and mute controls.
    /// Designed to work with Vivox SDK (16.6.0) via Unity Gaming Services.
    /// </summary>
    public class VivoxChatService : IChatService
    {
        private VoiceChatConfig config;
        private GameObject audioOrigin;
        private string positionalChannel;
        private bool isConnected;
        private string userId;

        public bool IsConnected => isConnected;

        public event Action<string, string, string> OnTextMessageReceived;

        public void Configure(VoiceChatConfig config)
        {
            this.config = config;

            if (!string.Equals(config.Provider, "Vivox", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[VivoxVoiceChatService] Unexpected provider: {config.Provider}");
            }

            if (!string.IsNullOrEmpty(config.Domain))
            {
                Debug.Log($"[VivoxVoiceChatService] Vivox domain set to: {config.Domain}");
            }

            if (config.Custom != null)
            {
                foreach (var kvp in config.Custom)
                    Debug.Log($"[VivoxVoiceChatService] Custom config: {kvp.Key} = {kvp.Value}");
            }
        }

        public async void Initialize(string userId)
        {
            this.userId = userId;

            if (VivoxService.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[Vivox] Already logged in. Skipping login.");
                return;
            }

            try
            {
                await VivoxService.Instance.LoginAsync(new LoginOptions { DisplayName = userId });

                VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
                VivoxService.Instance.DirectedMessageReceived -= OnDirectedMessageReceived;

                VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
                VivoxService.Instance.DirectedMessageReceived += OnDirectedMessageReceived;

                isConnected = true;
                Debug.Log($"[Vivox] Logged in as {userId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Vivox] Login failed: {ex.Message}");
                isConnected = false;
            }
        }


        public async void JoinChannel(string channelName, bool positional = false, GameObject trackedObject = null)
        {
            if (!isConnected)
            {
                Debug.LogWarning("[Vivox] Not connected — cannot join channel.");
                return;
            }

            try
            {
                ChatCapability capability = positional ? ChatCapability.AudioOnly : ChatCapability.TextAndAudio;
                await VivoxService.Instance.JoinGroupChannelAsync(channelName, capability);

                if (positional)
                {
                    positionalChannel = channelName;

                    if (trackedObject != null)
                        audioOrigin = trackedObject;
                }

                Debug.Log($"[Vivox] Joined channel: {channelName} (Positional: {positional})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Vivox] Failed to join channel: {ex.Message}");
            }
        }

        public async void LeaveChannel(string channelName)
        {
            try
            {
                await VivoxService.Instance.LeaveChannelAsync(channelName);
                Debug.Log($"[Vivox] Left channel: {channelName}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Vivox] Could not leave channel: {ex.Message}");
            }
        }

        public void Set3DPosition(Vector3 position)
        {
            if (!VivoxService.Instance.IsLoggedIn || audioOrigin == null || string.IsNullOrEmpty(positionalChannel))
            {
                Debug.LogWarning("[Vivox] Cannot set 3D position — missing context.");
                return;
            }

            audioOrigin.transform.position = position;
            VivoxService.Instance.Set3DPosition(audioOrigin, positionalChannel);
        }

        public void SetMuted(bool muted)
        {
            if (!VivoxService.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[Vivox] Cannot change mute state — not logged in.");
                return;
            }

            if (muted)
            {
                VivoxService.Instance.MuteInputDevice();
                Debug.Log("[Vivox] Microphone muted.");
            }
            else
            {
                VivoxService.Instance.UnmuteInputDevice();
                Debug.Log("[Vivox] Microphone unmuted.");
            }
        }

        private async Task SendTextMessageAsync(string message, string channelName)
        {
            if (!VivoxService.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[Vivox] Cannot send text message — not logged in.");
                return;
            }

            try
            {
                await VivoxService.Instance.SendChannelTextMessageAsync(channelName, message);
                Debug.Log($"[Vivox] Sent message to channel {channelName}: {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Vivox] Failed to send message to channel {channelName}: {ex.Message}");
            }
        }

        private async Task SendDirectMessageAsync(string message, string playerId)
        {
            if (!VivoxService.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[Vivox] Cannot send direct message — not logged in.");
                return;
            }

            try
            {
                await VivoxService.Instance.SendDirectTextMessageAsync(playerId, message);
                Debug.Log($"[Vivox] Sent direct message to {playerId}: {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Vivox] Failed to send direct message to {playerId}: {ex.Message}");
            }
        }

        private void OnChannelMessageReceived(VivoxMessage message)
        {
            string senderId = message.SenderPlayerId;
            string text = message.MessageText;
            string channelName = message.ChannelName;

            Debug.Log($"[Vivox] Channel message from {senderId} in {channelName}: {text}");
            OnTextMessageReceived?.Invoke(senderId, text, channelName);
        }

        private void OnDirectedMessageReceived(VivoxMessage message)
        {
            string senderId = message.SenderPlayerId;
            string text = message.MessageText;

            Debug.Log($"[Vivox] Direct message from {senderId}: {text}");
            OnTextMessageReceived?.Invoke(senderId, text, null);
        }

        public async void Disconnect()
        {
            if (VivoxService.Instance.IsLoggedIn)
            {
                await VivoxService.Instance.LogoutAsync();
                Debug.Log("[Vivox] Logged out of Vivox.");
            }

            VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
            VivoxService.Instance.DirectedMessageReceived -= OnDirectedMessageReceived;

            isConnected = false;
        }

        public void SendMessage(string message, string channelOrUser, bool direct)
        {
            if (direct)
                _ = SendDirectMessageAsync(message, channelOrUser);
            else
                _ = SendTextMessageAsync(message, channelOrUser);
        }
    }

#endif

}