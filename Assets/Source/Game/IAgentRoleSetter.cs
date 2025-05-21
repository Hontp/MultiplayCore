namespace FrameLabs.Multiplayer.Game
{
    public interface IAgentRoleSetter<TRole> : IAgentRole<TRole>
    {
        void SetRole(TRole role);
    }
}