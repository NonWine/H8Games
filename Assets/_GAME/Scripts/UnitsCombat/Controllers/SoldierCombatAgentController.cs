using UnityEngine;
using System.Collections.Generic;

public class SoldierCombatAgentController : BaseCombatAgentController<SoldierRuntimeModel>
{
    private readonly SoldierStateMachine stateMachine;
    private readonly SoldierFormationHandler formationModule;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;

    private SquadRootView squadRootView;
    private FormationSlot assignedSlot;
    
    public SoldierCombatAgentController(
        SoldierRuntimeModel runtimeModel,
        CombatUnitModules modules,
        SoldierStateMachine stateMachine,
        SoldierFormationHandler formationModule,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        ITargetTrackerHandler targetTrackerHandler,
        UnitRotatorService unitRotatorService,
        ITargetReservationHandler targetReservationHandler)
        : base(runtimeModel, modules, unitRotatorService, targetTrackerHandler, targetReservationHandler)
    {
        this.stateMachine = stateMachine;
        this.formationModule = formationModule;
        this.squadSlotPositionProvider = squadSlotPositionProvider;
    }
     
    protected override void TickBehaviour()
    {
        stateMachine.Tick();
        State = formationModule.UpdateFormation(transform, Time.deltaTime, State, squadRootView, assignedSlot);
    }
    
    public void AssignSquad(SquadRootView squadRootView)
    {
        this.squadRootView = squadRootView;
    }

    public void AssignSlot(FormationSlot slot)
    {
        assignedSlot = slot;
        formationModule.Reset();
    }

    public void ClearSquad(SquadRootView owner)
    {
        if (squadRootView != owner)
        {
            return;
        }

        assignedSlot = null;
        squadRootView = null;
        formationModule.Reset();
    }

    public bool IsInAssignedSlot(float threshold)
    {
        if (assignedSlot == null)
        {
            return false;
        }

        Vector3 targetPosition = squadSlotPositionProvider.GetSlotWorldPosition(assignedSlot);
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
