using FrameLabs.Multiplayer.Core;

namespace FrameLabs.Multiplayer.Game
{
    /// <summary>
    /// Default team-based agent classification system.
    /// Classifies agents as Ally, Hostile, or Neutral based on team ID.
    /// </summary>
    public class TeamBasedAgentClassifier : IAgentClassification
    {
        private readonly ITeamAssignable self;

        /// <summary>
        /// Constructs a classifier using the current agent's team info.
        /// </summary>
        public TeamBasedAgentClassifier(ITeamAssignable source)
        {
            self = source;
        }

        public AgentRelationship Classify(INetworkAgent target)
        {
            if (target == null)
                return AgentRelationship.Neutral;

            if (target is ITeamAssignable targetTeam)
            {
                if (targetTeam.TeamId == self.TeamId)
                    return AgentRelationship.Ally;
                else
                    return AgentRelationship.Hostile;
            }

            return AgentRelationship.Neutral;
        }
    }
}
