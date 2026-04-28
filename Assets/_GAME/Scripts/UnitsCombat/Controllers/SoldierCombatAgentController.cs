using UnityEngine;
using System.Collections.Generic;

public class SoldierCombatAgentController : BaseCombatAgentController<SoldierRuntimeModel>
{
    private readonly SoldierStateMachine stateMachine;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;
    
    public SoldierCombatAgentController(
        SoldierRuntimeModel runtimeModel,
        CombatUnitModules modules,
        SoldierStateMachine stateMachine,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        ITargetTrackerHandler targetTrackerHandler,
        ITargetReservationHandler targetReservationHandler)
        : base(runtimeModel, modules, targetTrackerHandler, targetReservationHandler)
    {
        this.stateMachine = stateMachine;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
    }
     
    protected override void TickBehaviour()
    {
        stateMachine.Tick();
    }
    
    public void AssignSquad(SquadRootView squadRootView)
    {
        runtimeModel.AssignSquad(squadRootView);
    }

    public void AssignSlot(FormationSlot slot)
    {
        runtimeModel.AssignSlot(slot);
    }

    public void ClearSquad(SquadRootView owner)
    {
        runtimeModel.ClearSquad(owner);
    }

    public bool IsInAssignedSlot(float threshold)
    {
        if (runtimeModel.AssignedSlot == null)
        {
            return false;
        }

        Vector3 targetPosition = squadSlotPositionProvider.GetSlotWorldPosition(runtimeModel.AssignedSlot);
        Vector3 delta = targetPosition - transform.position;
        delta.y = 0f;
        return delta.sqrMagnitude <= threshold * threshold;
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SoldierIdleState>();
    }

    protected override void ChangeToDeadState()
    {
        stateMachine.ChangeState<SoldierDeadState>();
    }
    
}
