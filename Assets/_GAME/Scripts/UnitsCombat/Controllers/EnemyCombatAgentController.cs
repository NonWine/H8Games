public class EnemyCombatAgentController : BaseCombatAgentController<EnemyRuntimeModel>
{
    private readonly EnemyStateMachine stateMachine;

    public EnemyCombatAgentController(
        EnemyRuntimeModel runtimeModel,
        CombatUnitModules modules,
        EnemyStateMachine stateMachine,
        ITargetTrackerHandler targetTrackerHandler,
        ITargetReservationHandler targetReservationHandler)
        : base(runtimeModel, modules, targetTrackerHandler, targetReservationHandler)
    {
        this.stateMachine = stateMachine;
    }

    // Enemies are scene-placed and never go through the pool's Spawn() path,
    // so enter the initial state here to start the state machine.
    public override void Initialize()
    {
        base.Initialize();
        ChangeToIdleState();
    }

    protected override void TickBehaviour() => stateMachine.Tick();
    protected override void ChangeToIdleState() => stateMachine.ChangeState<EnemyIdleState>();
    protected override void ChangeToDeadState() => stateMachine.ChangeState<EnemyDeadState>();
}
