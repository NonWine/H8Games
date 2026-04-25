using UnityEngine;

public abstract class AgentRuntimeModel : IAliveState
{
    protected AgentRuntimeModel(
        BaseCombatAgentView view,
        UnitStats unitStats,
        ITargetTrackerHandler targetTracker)
    {
        View = view;
        UnitStats = unitStats;
        TargetTracker = targetTracker;
    }

    public BaseCombatAgentView View { get; }
    public UnitStats UnitStats { get; }
    public ITargetTrackerHandler TargetTracker { get; }
    public bool IsAlive { get; set; } = true;
    public Transform Transform => View.transform;
    public ICombatTarget CurrentTarget => TargetTracker.CurrentTarget;
    public bool HasValidTarget => TargetTracker.IsCurrentTargetValid();
}
