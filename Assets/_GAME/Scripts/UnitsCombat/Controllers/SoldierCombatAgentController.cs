using UnityEngine;
using Zenject;

public class SoldierCombatAgentController : BaseCombatAgentController<SoldierRuntimeModel>
{
    private readonly SoldierStateMachine stateMachine;

    protected override CombatSide Side => CombatSide.Ally;

    public SoldierCombatAgentController(
        SoldierRuntimeModel runtimeModel,
        CombatUnitModules modules,
        SoldierStateMachine stateMachine,
        ITargetTrackerHandler targetTrackerHandler,
        ITargetReservationHandler targetReservationHandler,
        SignalBus signalBus)
        : base(runtimeModel, modules, targetTrackerHandler, targetReservationHandler, signalBus)
    {
        this.stateMachine = stateMachine;
    }

    protected override void ResetView()
    {
        var view = (BaseCombatAgentView)runtimeModel.View;
        view.RagdollView.ResetStateImmediate();
        view.NavMeshAgent.enabled = true;
    }

    protected override void TeardownView()
    {
        runtimeModel.View.NavMeshAgent.enabled = false;
    }

    protected override void TickBehaviour() => stateMachine.Tick();

    public void AssignSquad(SquadRootView squadRootView) => runtimeModel.AssignSquad(squadRootView);
    public void AssignSlot(FormationSlot slot) => runtimeModel.AssignSlot(slot);
    public void ClearSquad(SquadRootView owner) => runtimeModel.ClearSquad(owner);
    
    protected override void ChangeToIdleState() => stateMachine.ChangeState<SoldierIdleState>();
    protected override void ChangeToDeadState() => stateMachine.ChangeState<SoldierDeadState>();
}
