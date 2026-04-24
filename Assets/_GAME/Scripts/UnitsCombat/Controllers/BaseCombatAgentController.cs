using System;
using UnityEngine;
using Zenject;

public class BaseCombatAgentController : ITickable , IInitializable, IDisposable
{
    public UnitState State { get; protected set; } = UnitState.Idle;
    private BaseCombatAgentView baseCombatAgentView;
    protected UnitStats unitStats;
    protected CombatUnitModules modules;
    
    public BaseCombatAgentController(BaseCombatAgentView baseCombatAgentView, ModulesFactoryCollection modulesFactoryCollection)
    {
        this.baseCombatAgentView =  baseCombatAgentView;
        unitStats = baseCombatAgentView.unitConfig.CreateRuntimeStats();
        var unitModuleFactory = modulesFactoryCollection.Create(baseCombatAgentView.unitConfig.unitModuleType);
        modules = unitModuleFactory.Create(new CombatUnitModulesArgs(baseCombatAgentView, unitStats));

    }
    
    public void Tick()
    {
        modules.Animation.Apply(State);
    }

    public void Initialize()
    {
        modules.Health.Died += OnDied;
        baseCombatAgentView.OnHit += ReceiveDamage;
        ApplyAttackAnimationSpeed();
        baseCombatAgentView.AttackAnimationEvents.AttackTriggered += HandleAttackAnimationTriggered;
    }


    private void ReceiveDamage(HitData hitData)
    {
        modules.Health.ApplyDamage(hitData.damage);
    }
    
    private void HandleAttackAnimationTriggered()
    {
        if (!baseCombatAgentView.IsAlive || State != UnitState.Attack || !modules.TargetTracker.IsCurrentTargetValid())
            return;

        ICombatTarget currentTarget = modules.TargetTracker.CurrentTarget;
        Vector3 attackOrigin = baseCombatAgentView.AttackPoint.position;

        if (!modules.ProjectileSpawner.Spawn(
                baseCombatAgentView.AttackPoint,
                currentTarget.transform,
                unitStats.ProjectileSpeed,
                () => modules.Attack.ApplyDamage(currentTarget, attackOrigin)))
        {
            modules.Attack.ApplyDamage(currentTarget, attackOrigin);
        }
    }
    
    protected void ApplyAttackAnimationSpeed()
    {
        modules.Animation.SetAttackAnimationSpeed(modules.Attack.GetAttackAnimationSpeed(baseCombatAgentView.AttackAnimationCycleDuration));
    }
    
    protected virtual void OnDied()
    {
        State = UnitState.Dead;
        modules.Death.HandleDeathAsync();
    }

    public void Dispose()
    {
        modules.TargetTracker.ReleaseCurrentTarget(baseCombatAgentView);
        modules.Reservation.ClearReservations();
        
        if (modules != null)
            modules.Health.Died -= OnDied;

        if (baseCombatAgentView != null && baseCombatAgentView.AttackAnimationEvents != null)
            baseCombatAgentView.AttackAnimationEvents.AttackTriggered -= HandleAttackAnimationTriggered;
    }
}