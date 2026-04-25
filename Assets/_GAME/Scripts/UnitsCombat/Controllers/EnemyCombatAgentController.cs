using UnityEngine;
using System.Collections.Generic;

public class EnemyCombatAgentController : BaseCombatAgentController
{
    private readonly CurrencyService currencyService;


    public EnemyCombatAgentController(
        BaseCombatAgentView baseCombatAgentView,
        ModulesFactoryCollection modulesFactoryCollection, 
        ITargetTrackerHandler targetTrackerHandler,
        UnitRotatorService unitRotatorService,
        ITargetReservationHandler targetReservationHandler,
        CurrencyService currencyService)
        : base(baseCombatAgentView, modulesFactoryCollection, unitRotatorService, targetTrackerHandler, targetReservationHandler)
    {
        this.currencyService = currencyService;
    }
    
    public void ResetRunTimeState()
    {
        IsActive = false;

        if (!IsAlive)
        {
            State = UnitState.Dead;
            return;
        }

        modules.ResetModules();

        State = UnitState.Idle;
    }

    public void Activate()
    {
        if (!IsAlive)
            return;

        IsActive = true;
        modules.Attack.RandomizeAttackAnimationSpeed();
        State = UnitState.Attack;
    }

    protected override void OnDied()
    {
        IsActive = false;
        currencyService.Add(unitStats.DeathReward);
        base.OnDied();
    }
}
