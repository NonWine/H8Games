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

    public override void Spawn(Vector3 position, Quaternion rotation)
    {
        var view = (BaseCombatAgentView)runtimeModel.View;
        view.RagdollView.ResetStateImmediate();
        view.NavMeshAgent.enabled = true;
        base.Spawn(position, rotation);
    }

    protected override void TickBehaviour() => stateMachine.Tick();

    public void AssignSquad(SquadRootView squadRootView) => runtimeModel.AssignSquad(squadRootView);
    public void AssignSlot(FormationSlot slot) => runtimeModel.AssignSlot(slot);
    public void ClearSquad(SquadRootView owner) => runtimeModel.ClearSquad(owner);
    
    protected override void ChangeToIdleState() => stateMachine.ChangeState<SoldierIdleState>();
    protected override void ChangeToDeadState() => stateMachine.ChangeState<SoldierDeadState>();
}
