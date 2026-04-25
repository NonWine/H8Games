using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BaseCombatAgentController : ITickable, IInitializable, IDisposable, ICombatTarget
{
    
    protected readonly BaseCombatAgentView baseCombatAgentView;
    protected readonly UnitStats unitStats;
    protected readonly CombatUnitModules modules;
    private   readonly UnitRotatorService  unitRotatorService;
    private   readonly ITargetTrackerHandler targetTracker;
    private   readonly ITargetReservationHandler _reservationHandlerAttackers;
    
    public ITargetReservationHandler reservationHandler => _reservationHandlerAttackers;
    public Transform transform => baseCombatAgentView.transform;
    public UnitState State { get; protected set; } = UnitState.Idle;

    public string UnitId { get; private set; }
    public bool IsActive { get; protected set; }
    public bool IsAlive => modules.Health.IsAlive;
    
    public event Action Died;
    public event Action<HitData> HitReceived;


    public BaseCombatAgentController(BaseCombatAgentView baseCombatAgentView, 
        ModulesFactoryCollection modulesFactoryCollection,
        UnitRotatorService unitRotatorService,
        ITargetTrackerHandler targetTracker,
        ITargetReservationHandler targetReservationHandler)
    {
        this.baseCombatAgentView = baseCombatAgentView;
        this.unitRotatorService = unitRotatorService;
        this.targetTracker = targetTracker;
        this._reservationHandlerAttackers = targetReservationHandler;
        var unitModuleFactory = modulesFactoryCollection.Create(baseCombatAgentView.unitConfig.unitModuleType);
        unitStats = baseCombatAgentView.unitConfig.CreateRuntimeStats();
        modules = unitModuleFactory.Create(new CombatUnitModulesArgs(baseCombatAgentView, unitStats));
    }

    public virtual void Tick()
    {
        targetTracker.UpdateTarget(State);
        modules.Tick(State, Time.deltaTime);
        
        if (targetTracker.CurrentTarget != null)
        {
            unitRotatorService.RotateTowards(baseCombatAgentView.transform, targetTracker.CurrentTarget.transform);
        }
    }

    public void Initialize()
    {
        modules.Health.Died += OnDied;
        baseCombatAgentView.AttackAnimationEvents.AttackTriggered += HandleAttackAnimationTriggered;

    }

    private void HandleAttackAnimationTriggered()
    {
        if (!modules.Health.IsAlive || State != UnitState.Attack || !targetTracker.IsCurrentTargetValid())
        {
            return;
        }
        
        modules.Attack.HandleAttack(targetTracker.CurrentTarget, baseCombatAgentView.AttackPoint,
            () => targetTracker.CurrentTarget.TakeDamage(unitStats.Damage, Vector3.zero));
    }

    public virtual void TakeDamage(float damage, Vector3 sourceWorldPosition)
    {
        HitData hitData = new HitData
        {
            damage = damage,
            sourceWorldPosition = sourceWorldPosition
        };

        HitReceived?.Invoke(hitData);
        modules.Health.ApplyDamage(hitData.damage);
        baseCombatAgentView.SetEmissionHitFlash();
    }
    
    public void SetIdentity(string unitId)
    {
        UnitId = unitId;
    }

    protected virtual void OnDied()
    {
        State = UnitState.Dead;
        modules.DisposeModules();
        Died?.Invoke();
        modules.Death.HandleDeathAsync().Forget();
    }

    public virtual void Dispose()
    {
        modules.Health.Died -= OnDied;
        baseCombatAgentView.AttackAnimationEvents.AttackTriggered -= HandleAttackAnimationTriggered;
        _reservationHandlerAttackers.ClearReservations();
        modules.DisposeModules();
    }
}

