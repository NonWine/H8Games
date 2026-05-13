using UnityEngine;

public abstract class AgentRuntimeModel : IAliveState
{
    protected AgentRuntimeModel(
        IAgentView view,
        UnitStats unitStats,
        ITargetTrackerHandler targetTracker)
    {
        View = view;
        UnitStats = unitStats;
        TargetTracker = targetTracker;
    }

    public IAgentView View { get; }
    public UnitStats UnitStats { get; }
    public ITargetTrackerHandler TargetTracker { get; }
    public bool IsAlive { get; set; } = true;
    public HitData LastHitData { get; set; }
    public Transform Transform => View.Transform;
    public ICombatTarget CurrentTarget => TargetTracker.CurrentTarget;
    public bool HasValidTarget => TargetTracker.IsCurrentTargetValid();
}
