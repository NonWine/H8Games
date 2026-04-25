using UnityEngine;
using System.Collections.Generic;

public class EnemyCombatAgentController : BaseCombatAgentController<EnemyRuntimeModel>
{
    private readonly EnemyStateMachine stateMachine;

    public EnemyCombatAgentController(
        EnemyRuntimeModel runtimeModel,
        CombatUnitModules modules,
        EnemyStateMachine stateMachine,
        ITargetTrackerHandler targetTrackerHandler,
        UnitRotatorService unitRotatorService,
        ITargetReservationHandler targetReservationHandler)
        : base(runtimeModel, modules, unitRotatorService, targetTrackerHandler, targetReservationHandler)
    {
        this.stateMachine = stateMachine;
    }

    protected override void TickBehaviour()
    {
        stateMachine.Tick();
    }
    
    public void ResetRunTimeState()
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<EnemyIdleState>();
    }

    protected override void ChangeToDeadState()
    {
        stateMachine.ChangeState<EnemyDeadState>();
    }
}
