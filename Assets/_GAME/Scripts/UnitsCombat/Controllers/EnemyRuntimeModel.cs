public class EnemyRuntimeModel : AgentRuntimeModel
{
    public EnemyRuntimeModel(BaseCombatAgentView view, UnitStats unitStats, ITargetTrackerHandler targetTracker)
        : base(view, unitStats, targetTracker)
    {
    }
}
