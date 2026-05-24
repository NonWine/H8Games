using UnityEngine;

public class SoldierCombatAgentController : BaseCombatAgentController<SoldierRuntimeModel>
{
    private readonly SoldierStateMachine stateMachine;
    private readonly ISoldierFormationMover formationMover;

    public SoldierCombatAgentController(
        SoldierRuntimeModel runtimeModel,
        CombatUnitModules modules,
        SoldierStateMachine stateMachine,
        ISoldierFormationMover formationMover,
        ITargetTrackerHandler targetTrackerHandler,
        ITargetReservationHandler targetReservationHandler)
        : base(runtimeModel, modules, targetTrackerHandler, targetReservationHandler)
    {
        this.stateMachine = stateMachine;
        this.formationMover = formationMover;
    }

    protected override void TickBehaviour() => stateMachine.Tick();

    public void AssignSquad(SquadRootView squadRootView) => runtimeModel.AssignSquad(squadRootView);
    public void AssignSlot(FormationSlot slot) => runtimeModel.AssignSlot(slot);
    public void ClearSquad(SquadRootView owner) => runtimeModel.ClearSquad(owner);

    // Soldier overrides placement only — base.Spawn handles target/module reset, IsAlive, and Idle transition.
    protected override void PlaceAtSpawn(Vector3 position, Quaternion rotation)
        => formationMover.TeleportTo(position, rotation);

    protected override void ChangeToIdleState() => stateMachine.ChangeState<SoldierIdleState>();
    protected override void ChangeToDeadState() => stateMachine.ChangeState<SoldierDeadState>();
}
