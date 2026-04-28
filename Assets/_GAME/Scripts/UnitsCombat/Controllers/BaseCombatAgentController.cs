using System;
using UnityEngine;
using Zenject;

public abstract class BaseCombatAgentController<TModel> : ITickable, IInitializable, IDisposable, ICombatTarget, IAgentController
    where TModel : AgentRuntimeModel
{
    protected readonly IAgentView agentView;
    protected readonly CombatUnitModules modules;
    protected readonly TModel runtimeModel;

    private readonly ITargetTrackerHandler targetTracker;
    private readonly ITargetReservationHandler reservationHandlerAttackers;

    public ITargetReservationHandler reservationHandler => reservationHandlerAttackers;
    public Transform transform => agentView.Transform;

    public string UnitId { get; private set; }
    public bool IsAlive => runtimeModel.IsAlive;

    public event Action Died;
    public event Action<HitData> HitReceived;

    protected BaseCombatAgentController(
        TModel runtimeModel,
        CombatUnitModules modules,
        ITargetTrackerHandler targetTracker,
        ITargetReservationHandler targetReservationHandler)
    {
        this.runtimeModel = runtimeModel;
        this.modules = modules;
        this.targetTracker = targetTracker;
        reservationHandlerAttackers = targetReservationHandler;
        agentView = runtimeModel.View;
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
        agentView.PlayHitFeedback();
    }

    public void SetIdentity(string unitId)
    {
        UnitId = unitId;
    }

    public void ResetState()
    {
        runtimeModel.IsAlive = true;
        modules.ResetModules();
        ChangeToIdleState();
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
