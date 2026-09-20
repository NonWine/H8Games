public class HeroRuntimeModel : AgentRuntimeModel, IAliveStateReader
{
    public HeroRuntimeModel(BaseCombatUnitView view, UnitStats unitStats, ITargetTrackerHandler targetTracker)
        : base(view, unitStats, targetTracker)
    {
    }
}
