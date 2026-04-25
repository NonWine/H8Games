using System;
using UnityEngine;
using Zenject;

public class BaseCombatAgentController : ITickable, IInitializable, IDisposable, ICombatTarget
{
    
    protected readonly BaseCombatAgentView baseCombatAgentView;
    protected readonly UnitStats unitStats;
    protected readonly CombatUnitModules modules;
    private   readonly UnitRotatorService  unitRotatorService;
    private   readonly ITargetTrackerHandler targetTracker;
    private   readonly ITargetReservationHandler reservationHandlerAttackers;
    private readonly AgentStateMachine stateMachine;
    public ITargetReservationHandler reservationHandler => reservationHandlerAttackers;
    public Transform transform => baseCombatAgentView.transform;

    public string UnitId { get; private set; }
    public bool IsActive { get; protected set; }
    public bool IsAlive => modules.Health.IsAlive;
    public UnitState State { get; protected set; } = UnitState.Idle;

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
        this.reservationHandlerAttackers = targetReservationHandler;
        var unitModuleFactory = modulesFactoryCollection.Create(baseCombatAgentView.unitConfig.unitModuleType);
        unitStats = baseCombatAgentView.unitConfig.CreateRuntimeStats();
        modules = unitModuleFactory.Create(new CombatUnitModulesArgs(baseCombatAgentView, unitStats));
    }

    public void Tick()
    {
        if(!modules.Health.IsAlive)
            return;
        
        TickTracking();
        TickModules();
        TickBehaviour();
    }

    protected virtual void TickTracking()
    {
        targetTracker.UpdateTarget(State);
        
        if (targetTracker.CurrentTarget != null)
        {
            unitRotatorService.RotateTowards(baseCombatAgentView.transform, targetTracker.CurrentTarget.transform);
        }
    }

    protected virtual void TickModules()
    {
        modules.Tick(Time.deltaTime);
    }

    protected virtual void TickBehaviour()
    {
        stateMachine.Tick();
    }
    
    public void Initialize()
    {
        modules.Health.Died += OnDied;
        stateMachine.ChangeState<AgentIdleState>();
    }

    public virtual void TakeDamage(float damage, Vector3 sourceWorldPosition)
    {
        if (!modules.Health.IsAlive)
        {
            return;
        }

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
        stateMachine.ChangeState<AgentDeadState>();
        Died?.Invoke();
    }

    public virtual void Dispose()
    {
        modules.Health.Died -= OnDied;
        reservationHandlerAttackers.ClearReservations();
        modules.DisposeModules();
    }
}

