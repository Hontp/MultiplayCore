namespace FrameLabs.Multiplayer.Core
{
    public interface ITeamAssignableSetter : ITeamAssignable
    {
        void SetTeam(int teamId);
    }
}