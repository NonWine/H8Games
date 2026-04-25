using System;
using UnityEngine;
using Zenject;

public abstract class BaseCombatAgentController<TModel> : ITickable, IInitializable, IDisposable, ICombatTarget, IAgentController
    where TModel : AgentRuntimeModel
{
    protected readonly BaseCombatAgentView baseCombatAgentView;
    protected readonly UnitStats unitStats;
    protected readonly CombatUnitModules modules;
    protected readonly TModel runtimeModel;

    private readonly UnitRotatorService unitRotatorService;
    private readonly ITargetTrackerHandler targetTracker;
    private readonly ITargetReservationHandler reservationHandlerAttackers;

    public ITargetReservationHandler reservationHandler => reservationHandlerAttackers;
    public Transform transform => baseCombatAgentView.transform;

    public string UnitId { get; private set; }
    public bool IsActive { get; protected set; }
    public bool IsAlive => modules.Health.IsAlive;
    public UnitState State { get; protected set; } = UnitState.Idle;

    public event Action Died;
    public event Action<HitData> HitReceived;

    protected BaseCombatAgentController(
        TModel runtimeModel,
        CombatUnitModules modules,
        UnitRotatorService unitRotatorService,
        ITargetTrackerHandler targetTracker,
        ITargetReservationHandler targetReservationHandler)
    {
        this.runtimeModel = runtimeModel;
        this.modules = modules;
        this.unitRotatorService = unitRotatorService;
        this.targetTracker = targetTracker;
        reservationHandlerAttackers = targetReservationHandler;
        baseCombatAgentView = runtimeModel.View;
        unitStats = runtimeModel.UnitStats;
    }

    public void Tick()
    {
        if (!modules.Health.IsAlive)
        {
            return;
        }
        
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
    }
    
    public void Initialize()
    {
        modules.Health.Died += OnDied;
        ChangeToIdleState();
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

    protected abstract void ChangeToIdleState();
    protected abstract void ChangeToDeadState();

    protected virtual void OnDied()
    {
        ChangeToDeadState();
        Died?.Invoke();
    }

    public virtual void Dispose()
    {
        modules.Health.Died -= OnDied;
        reservationHandlerAttackers.ClearReservations();
        modules.DisposeModules();
    }
}

public abstract class AgentRuntimeModel
{
    protected AgentRuntimeModel(
        BaseCombatAgentView view,
        UnitStats unitStats,
        ITargetTrackerHandler targetTracker)
    {
        View = view;
        UnitStats = unitStats;
        TargetTracker = targetTracker;
    }

    public BaseCombatAgentView View { get; }
    public UnitStats UnitStats { get; }
    public ITargetTrackerHandler TargetTracker { get; }

    public Transform Transform => View.transform;
    public ICombatTarget CurrentTarget => TargetTracker.CurrentTarget;
    public bool HasValidTarget => TargetTracker.IsCurrentTargetValid();
}

public class EnemyRuntimeModel : AgentRuntimeModel
{
    public EnemyRuntimeModel(BaseCombatAgentView view, UnitStats unitStats, ITargetTrackerHandler targetTracker)
        : base(view, unitStats, targetTracker)
    {
    }
}

public class SoldierRuntimeModel : AgentRuntimeModel
{
    public SoldierRuntimeModel(BaseCombatAgentView view, UnitStats unitStats, ITargetTrackerHandler targetTracker)
        : base(view, unitStats, targetTracker)
    {
    }
}
