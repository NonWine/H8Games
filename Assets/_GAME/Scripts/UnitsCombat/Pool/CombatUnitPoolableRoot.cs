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
        // controller.Spawn → TeleportTo handles position via the agent-safe path
        // (disable agent → set transform → sync Rigidbody → enable agent → Warp).
        controller.Spawn(spawnParams.Position, spawnParams.Rotation);
    }

    public void OnDespawned()
    {
        // Clear runtime state (IsAlive=false stops Tick) so this pooled instance
        // doesn't keep running between despawn and next spawn.
        controller.Despawn();
        pool = null;
    }

    public void Dispose() => pool?.Despawn(this);
}
