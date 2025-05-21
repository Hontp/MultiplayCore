namespace FrameLabs.Multiplayer.Game
{
    public interface IAgentRole<TRole>
    {
        TRole Role { get; }
    }
}