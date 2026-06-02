using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class PickupService : IPickupService, ITickable, IDisposable
{
    private readonly IPickupSpawner spawner;
    private readonly IPickupDepositer depositer;
    private readonly IPickupCollector collector;
    private readonly IPickupRuntimeRegistry registry;
    private readonly IPickupCarrySink carrySink;

    public event Action<PickupCollectedEvent> Collected;
    public event Action<PickupSpawnedEvent>   Spawned;

    public PickupService(
        IPickupSpawner         spawner,
        IPickupDepositer       depositer,
        IPickupCollector       collector,
        IPickupRuntimeRegistry registry,
        IPickupCarrySink       carrySink)
    {
        this.spawner   = spawner;
        this.depositer = depositer;
        this.collector = collector;
        this.registry  = registry;
        this.carrySink = carrySink;

        this.spawner.Spawned     += OnSpawnerSpawned;
        this.collector.Collected += OnCollectorCollected;
    }

    public UniTask SpawnAsync(PickupSpawnRequest request, CancellationToken ct = default)
        => spawner.SpawnAsync(request, ct);

    public void TossDeposit(string pickupId, Vector3 origin, Transform target, Action onArrived)
        => depositer.TossDeposit(pickupId, origin, target, onArrived);

    public void Return(PickupItemController controller)
    {
        registry.RemoveAnimating(controller);
        registry.Despawn(controller);
    }

    public void Clear()
    {
        registry.Clear();
        carrySink.Clear();
    }

    public void Dispose()
    {
        spawner.Spawned     -= OnSpawnerSpawned;
        collector.Collected -= OnCollectorCollected;
        Clear();
    }

    public void Tick()
    {
        registry.TickAnimations(Time.deltaTime);
        collector.Tick();
    }

    private void OnSpawnerSpawned(PickupSpawnedEvent e) => Spawned?.Invoke(e);

    private void OnCollectorCollected(PickupCollectedEvent e) => Collected?.Invoke(e);
}
