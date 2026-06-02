using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PickupSpawner : IPickupSpawner
{
    private readonly PickupCatalog catalog;
    private readonly PickupVisualConfig defaultVisuals;
    private readonly PickupViewPools pools;
    private readonly PickupItemController.Factory controllerFactory;
    private readonly IPickupRuntimeRegistry registry;

    public event Action<PickupSpawnedEvent> Spawned;

    public PickupSpawner(
        PickupCatalog                catalog,
        PickupVisualConfig           defaultVisuals,
        PickupViewPools              pools,
        PickupItemController.Factory controllerFactory,
        IPickupRuntimeRegistry       registry)
    {
        this.catalog           = catalog;
        this.defaultVisuals    = defaultVisuals;
        this.pools             = pools;
        this.controllerFactory = controllerFactory;
        this.registry          = registry;
    }

    public async UniTask SpawnAsync(PickupSpawnRequest request, CancellationToken ct = default)
    {
        if (!catalog.TryGet(request.PickupId, out _, out var overrideVisuals))
            return;

        var visuals = overrideVisuals ?? defaultVisuals;

        for (var i = 0; i < request.Amount; i++)
        {
            await UniTask.CompletedTask;

            var view       = pools.Spawn(request.PickupId);
            var controller = controllerFactory.Create(view, visuals);
            var scatterDir = request.ScatterDirection ?? GetRandomScatterDirection();

            controller.InitializeAsWorldItem(request.PickupId, 1, request.Position, scatterDir);
            registry.AddWorld(controller);
        }

        Spawned?.Invoke(new PickupSpawnedEvent(request.PickupId, request.Amount, request.Position));
    }

    private static Vector3 GetRandomScatterDirection()
    {
        var dir2D = UnityEngine.Random.insideUnitCircle;

        if (dir2D.sqrMagnitude <= 0.0001f)
            dir2D = Vector2.right;
        else
            dir2D.Normalize();

        return new Vector3(dir2D.x, 0f, dir2D.y);
    }
}
