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
        controller.Spawn(spawnParams.Position, spawnParams.Rotation);
    }

    public void OnDespawned()
    {
        controller.Despawn();
        pool = null;
    }

    public void Dispose() => pool?.Despawn(this);
}
