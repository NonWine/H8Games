using Zenject;

public class EnemyCombatAgentController : BaseCombatAgentController<EnemyRuntimeModel>
{
    private readonly EnemyStateMachine stateMachine;

    protected override CombatSide Side => CombatSide.Enemy;

    public EnemyCombatAgentController(
        EnemyRuntimeModel runtimeModel,
        CombatUnitModules modules,
        EnemyStateMachine stateMachine,
        ITargetTrackerHandler targetTrackerHandler,
        ITargetReservationHandler targetReservationHandler,
        SignalBus signalBus)
        : base(runtimeModel, modules, targetTrackerHandler, targetReservationHandler, signalBus)
    {
        this.stateMachine = stateMachine;
    }
    
    public override void Initialize()
    {
        base.Initialize();
        ChangeToIdleState();
    }

    // Enemies are authored into the scene rather than pooled, so nothing used to
    // undo their death: the ragdoll stayed on the floor and the agent stayed off.
    // A group reset now respawns them through the same Spawn() path the pooled
    // soldiers use, and that path needs the view put back the way it started.
    protected override void ResetView()
    {
        var view = (BaseCombatAgentView)runtimeModel.View;
        view.RagdollView.ResetStateImmediate();
        view.NavMeshAgent.enabled = true;
    }

    protected override void TickBehaviour() => stateMachine.Tick();
    protected override void ChangeToIdleState() => stateMachine.ChangeState<EnemyIdleState>();
    protected override void ChangeToDeadState() => stateMachine.ChangeState<EnemyDeadState>();
}
