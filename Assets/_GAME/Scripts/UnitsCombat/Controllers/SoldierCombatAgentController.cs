using UnityEngine;

public class SoldierCombatAgentController : BaseCombatAgentController<SoldierRuntimeModel>
{
    private readonly SoldierStateMachine stateMachine;

    public SoldierCombatAgentController(
        SoldierRuntimeModel runtimeModel,
        CombatUnitModules modules,
        SoldierStateMachine stateMachine,
        ITargetTrackerHandler targetTrackerHandler,
        ITargetReservationHandler targetReservationHandler)
        : base(runtimeModel, modules, targetTrackerHandler, targetReservationHandler)
    {
        this.stateMachine = stateMachine;
    }

    protected override void TickBehaviour() => stateMachine.Tick();

    public void AssignSquad(SquadRootView squadRootView) => runtimeModel.AssignSquad(squadRootView);
    public void AssignSlot(FormationSlot slot) => runtimeModel.AssignSlot(slot);
    public void ClearSquad(SquadRootView owner) => runtimeModel.ClearSquad(owner);
    
    protected override void ChangeToIdleState() => stateMachine.ChangeState<SoldierIdleState>();
    protected override void ChangeToDeadState() => stateMachine.ChangeState<SoldierDeadState>();
}
