using System;
using UnityEngine;
using Zenject;

public abstract class BaseCombatAgentController<TModel> : ITickable, IInitializable, IDisposable, IAgentController
    where TModel : AgentRuntimeModel
{
    protected readonly IAgentView agentView;
    protected readonly CombatUnitModules modules;
    protected readonly TModel runtimeModel;

    private readonly ITargetTrackerHandler targetTracker;
    private readonly ITargetReservationHandler reservationHandlerAttackers;

    public Transform Transform => agentView.Transform;
    public bool IsAlive => runtimeModel.IsAlive;

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

    public void Initialize()
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
        PlaceAtSpawn(position, rotation);
        targetTracker.Reset();
        modules.ResetModules();
        runtimeModel.IsAlive = true;   // flip true LAST so Tick can't fire mid-setup
        ChangeToIdleState();
    }

    // Override in subclasses that need agent-safe placement (e.g. NavMeshAgent.Warp).
    // Default just sets the view transform.
    protected virtual void PlaceAtSpawn(Vector3 position, Quaternion rotation)
    {
        agentView.Transform.SetPositionAndRotation(position, rotation);
    }

    public virtual void Despawn()
    {
        // Stop Tick(), reset target so pooled instance starts clean on next Spawn.
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
