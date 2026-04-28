using System;
using UnityEngine;
using Zenject;

public class CombatUnitPoolableRoot<TController> : MonoBehaviour, IPoolable<AgentSpawnParams, IMemoryPool>, IDisposable
    where TController : class, IAgentController
{
    [Inject] PoolableManager poolableManager;
    private TController controller;
    private IMemoryPool pool;

    public TController Controller => controller;

    [Inject]
    public void Construct(TController controller)
    {
        this.controller = controller;
    }

    public void OnSpawned(AgentSpawnParams spawnParams, IMemoryPool pool)
    {
        this.pool = pool;

        transform.SetPositionAndRotation(spawnParams.Position, spawnParams.Rotation);
        controller.SetIdentity(spawnParams.UnitId);
        controller.ResetState();
        poolableManager.TriggerOnSpawned();
    }

    public void OnDespawned()
    {
        pool = null;
        poolableManager.TriggerOnDespawned();
    }

    public void Dispose()
    {
        pool?.Despawn(this);
    }
}
