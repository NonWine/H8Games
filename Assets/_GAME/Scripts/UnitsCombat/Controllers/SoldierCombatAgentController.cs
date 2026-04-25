using UnityEngine;
using System.Collections.Generic;

public class SoldierCombatAgentController : BaseCombatAgentController
{
    private readonly SoldierFormationHandler formationModule;
    private readonly ISquadSlotPositionProvider squadSlotPositionProvider;

    private SquadRootView squadRootView;
    private FormationSlot assignedSlot;
    
    public SoldierCombatAgentController(
        BaseCombatAgentView baseCombatAgentView,
        ModulesFactoryCollection modulesFactoryCollection,
        SquadFollowSettings squadFollowSettings,
        ISquadSlotPositionProvider squadSlotPositionProvider,
        ISquadMovementStateReader movementStateReader,
        ITargetTrackerHandler targetTrackerHandler,
        UnitRotatorService unitRotatorService,
        ITargetReservationHandler targetReservationHandler)
        : base(baseCombatAgentView, modulesFactoryCollection, unitRotatorService, targetTrackerHandler, targetReservationHandler)
    {
        this.squadSlotPositionProvider = squadSlotPositionProvider;
        formationModule = new SoldierFormationHandler(
            movementStateReader,
            squadSlotPositionProvider,
            squadFollowSettings,
            baseCombatAgentView.GetInstanceID());
    }
    
    public override void Tick()
    {
        if (!IsAlive)
        {
            return;
        }
        
        State = formationModule.UpdateFormation(transform, Time.deltaTime, State, squadRootView, assignedSlot);
        base.Tick();
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

    public void ResetRunTimeState()
    {
        formationModule.Reset();
        modules.ResetModules();
        State = UnitState.Idle;
    }
}
