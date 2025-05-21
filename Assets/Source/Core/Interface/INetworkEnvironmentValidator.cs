namespace FrameLabs.Multiplayer.Core
{
    public interface INetworkEnvironmentValidator
    {
        void ValidateForRelay();
        void ValidateGlobal();
    }
}