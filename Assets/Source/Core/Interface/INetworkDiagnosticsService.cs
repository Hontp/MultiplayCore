namespace FrameLabs.Multiplayer.Core
{
    /// <summary>
    /// Provides access to per-client network health metrics.
    /// </summary>
    public interface INetworkDiagnosticsService
    {
        /// <summary>
        /// Returns the measured round-trip time (RTT) in milliseconds for the given client.
        /// </summary>
        float GetRTT(ulong clientId);

        /// <summary>
        /// Returns the estimated packet loss (%) for the given client (0–100).
        /// </summary>
        float GetPacketLoss(ulong clientId);

        /// <summary>
        /// Returns the transport backend name (e.g., NGO, UnityRelay, EOS).
        /// </summary>
        string GetTransportName();

        /// <summary>
        /// Returns true if the diagnostics backend is available and ready.
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// True if RTT (round-trip time) is available.
        /// </summary>
        bool SupportsRTT { get; }

        /// <summary>
        /// True if packet loss metrics are available.
        /// </summary>
        bool SupportsPacketLoss { get; }

        void Update();
    }

    /// <summary>
    /// Empty placeholder diagnostic object.
    /// Will always return default fallback values and indicate diagnostics are unsupported.
    /// Safe to use in offline or unsupported environments.
    /// </summary>
    public class NoDiagnosticsService : INetworkDiagnosticsService
    {
        public float GetRTT(ulong clientId) => -1f;
        public float GetPacketLoss(ulong clientId) => -1f;
        public string GetTransportName() => "Unavailable";
        public bool IsSupported => false;
        public bool SupportsRTT => false;
        public bool SupportsPacketLoss => false;
        public void Update() { }
    }

}
