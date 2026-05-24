using UnityEngine;

public abstract class AgentRuntimeModel : IAliveState
{
    private readonly ITargetTrackerHandler targetTracker;

    protected AgentRuntimeModel(IAgentView view, UnitStats unitStats, ITargetTrackerHandler targetTracker)
    {
        View = view;
        UnitStats = unitStats;
        this.targetTracker = targetTracker;
    }

    public IAgentView View { get; }
    public UnitStats UnitStats { get; }

    // Default false: pool-warmed (not yet spawned) items must not tick.
    // Spawn() flips this to true after position/identity/state are set up.
    public bool IsAlive { get; set; } = false;
    public HitData LastHitData { get; set; }
    public Transform Transform => View.Transform;

    // Read-only proxies used by state machines.
    public ICombatTarget CurrentTarget => targetTracker.CurrentTarget;
    public bool HasValidTarget => targetTracker.IsCurrentTargetValid();
}
