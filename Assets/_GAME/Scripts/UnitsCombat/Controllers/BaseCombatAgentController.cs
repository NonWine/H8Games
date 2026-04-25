using System;
using UnityEngine;
using Zenject;

public abstract class BaseCombatAgentController<TModel> : ITickable, IInitializable, IDisposable, ICombatTarget, IAgentController
    where TModel : AgentRuntimeModel
{
    protected readonly BaseCombatAgentView baseCombatAgentView;
    protected readonly CombatUnitModules modules;
    protected readonly TModel runtimeModel;

    private readonly UnitRotatorService unitRotatorService;
    private readonly ITargetTrackerHandler targetTracker;
    private readonly ITargetReservationHandler reservationHandlerAttackers;

    public ITargetReservationHandler reservationHandler => reservationHandlerAttackers;
    public Transform transform => baseCombatAgentView.transform;

    public string UnitId { get; private set; }
    public bool IsActive => baseCombatAgentView.gameObject.activeInHierarchy;
    public bool IsAlive => runtimeModel.IsAlive;
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
    }

    public void Tick()
    {
        if (!runtimeModel.IsAlive)
        {
            return;
        }
        TickTracking();
        TickModules();
        TickBehaviour();
    }

    protected virtual void TickTracking()
    {
        targetTracker.UpdateTarget();
        
        if (targetTracker.CurrentTarget != null)
        {
            unitRotatorService.RotateTowards(baseCombatAgentView.transform, targetTracker.CurrentTarget.transform);
        }
    }

    protected virtual void TickModules()
    {
        modules.Tick(Time.deltaTime);
    }
    
    public void Initialize()
    {
        modules.Health.Died += OnDied;
        ChangeToIdleState();
    }

    public virtual void TakeDamage(float damage, Vector3 sourceWorldPosition)
    {
        if (!runtimeModel.IsAlive)
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
    protected abstract void TickBehaviour();

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
