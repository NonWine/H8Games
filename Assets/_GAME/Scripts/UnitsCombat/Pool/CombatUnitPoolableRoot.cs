using System;
using UnityEngine;
using Zenject;

public class CombatUnitPoolableRoot<TController> : MonoBehaviour, IPoolable<AgentSpawnParams, IMemoryPool>, IDisposable
    where TController : class, IAgentController
{
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

    }

    public void OnDespawned()
    {
        pool = null;
    }

    public void Dispose()
    {
        pool?.Despawn(this);
    }
}
