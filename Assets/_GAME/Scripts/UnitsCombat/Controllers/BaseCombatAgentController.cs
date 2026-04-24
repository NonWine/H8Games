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
    private   readonly ProjectileVisualSpawner projectileSpawner;
    private   readonly ITargetTrackerHandler targetTracker;
    private   readonly float projectileSpeed;
    public ITargetReservation Reservation => modules.Reservation;
    public Transform transform => baseCombatAgentView.transform;
    public UnitState State { get; protected set; } = UnitState.Idle;

    public string UnitId { get; private set; }
    public bool IsAlive => modules.Health.IsAlive;
    public int ReservationCount => Reservation.ReservationCount;
    public event Action Died;
    public event Action<HitData> HitReceived;


    public BaseCombatAgentController(BaseCombatAgentView baseCombatAgentView, ModulesFactoryCollection modulesFactoryCollection)
    {
        this.baseCombatAgentView = baseCombatAgentView;
        unitStats = baseCombatAgentView.unitConfig.CreateRuntimeStats();
        var unitModuleFactory = modulesFactoryCollection.Create(baseCombatAgentView.unitConfig.unitModuleType);
        modules = unitModuleFactory.Create(new CombatUnitModulesArgs(baseCombatAgentView, unitStats));
    }

    public virtual void Tick()
    {
        modules.TargetTracker.UpdateTarget(State);
        modules.Tick(State, Time.deltaTime);
        
        if (modules.TargetTracker.CurrentTarget != null)
        {
            unitRotatorService.RotateTowards(baseCombatAgentView.transform, modules.TargetTracker.CurrentTarget.transform);
        }
    }

    public void Initialize()
    {
        modules.Health.Died += OnDied;
        baseCombatAgentView.AttackAnimationEvents.AttackTriggered += HandleAttackAnimationTriggered;

    }

    public virtual void GetDamage(float damage, Vector3 sourceWorldPosition)
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

        modules.DisposeModules();
    }
    
    private void HandleAttackAnimationTriggered()
    {
        if (!modules.Health.IsAlive || State != UnitState.Attack || !targetTracker.IsCurrentTargetValid())
        {
            return;
        }

        ICombatTarget currentTarget = targetTracker.CurrentTarget;
        Vector3 attackOrigin = baseCombatAgentView.AttackPoint.position;

        projectileSpawner.Spawn(baseCombatAgentView.AttackPoint,
            currentTarget.transform,
            projectileSpeed,
            () => modules.Attack.ApplyDamage(currentTarget, attackOrigin));
    }
}
