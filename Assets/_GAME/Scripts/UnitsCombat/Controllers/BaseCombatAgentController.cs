using System;
using UnityEngine;
using Zenject;

public abstract class BaseCombatAgentController<TModel> : ITickable, IInitializable, IDisposable, IAgentController , ITargetSelectionCandidate
    where TModel : AgentRuntimeModel
{
    protected readonly IAgentView agentView;
    protected readonly CombatUnitModules modules;
    protected readonly TModel runtimeModel;

    private readonly ITargetTrackerHandler targetTracker;
    private readonly ITargetReservationHandler reservationHandlerAttackers;

    public Vector3 Position => agentView.Transform.position;
    public bool IsAlive => runtimeModel.IsAlive;
    public int ReservationCount => reservationHandlerAttackers.ReservationCount;
    public Transform transform => agentView.Transform;

    public event Action Died;

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

        targetTracker.UpdateTarget();
        modules.Tick(Time.deltaTime);
        TickBehaviour();
    }

    public virtual void Initialize()
    {
        modules.Health.Died += OnDied;
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

        runtimeModel.LastHitData = hitData;
        ParticlePool.Instance.PlayHit(agentView.Transform.position);
        modules.Health.ApplyDamage(hitData.damage);
        agentView.PlayHitFeedback();
    }

    public virtual void Spawn(Vector3 position, Quaternion rotation)
    {
        transform.transform.rotation = rotation;
        agentView.NavMeshAgent.Warp(position);
        targetTracker.Reset();
        modules.ResetModules();
        runtimeModel.IsAlive = true; 
        ChangeToIdleState();
    }

    public virtual void Despawn()
    {
        runtimeModel.IsAlive = false;
        targetTracker.Reset();
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
