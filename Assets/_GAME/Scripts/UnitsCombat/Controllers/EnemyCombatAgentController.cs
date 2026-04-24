using UnityEngine;
using System.Collections.Generic;

public class EnemyCombatAgentController : BaseCombatAgentController
{
    private readonly CurrencyService currencyService;

    public bool IsActive { get; private set; }

    public EnemyCombatAgentController(
        BaseCombatAgentView baseCombatAgentView,
        ModulesFactoryCollection modulesFactoryCollection, 
        CurrencyService currencyService)
        : base(baseCombatAgentView, modulesFactoryCollection)
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
