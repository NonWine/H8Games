using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public sealed class PickupService : IPickupService, ITickable, IDisposable
{
    private readonly PickupCatalog              catalog;
    private readonly PickupVisualConfig         defaultVisuals;
    private readonly PickupServiceConfig        config;
    private readonly DiContainer                container;
    private readonly IPickupMagnetProvider      magnetProvider;
    private readonly IPickupAcceptanceFilter    acceptanceFilter;
    private readonly IPickupCarrySink           carrySink;
    private readonly List<PickupItemController> activeWorldItems     = new();
    private readonly List<PickupItemController> activeAnimatingItems = new();

    public event Action<PickupCollectedEvent> Collected;
    public event Action<PickupSpawnedEvent>   Spawned;

    public PickupService(
        PickupCatalog           catalog,
        PickupVisualConfig      defaultVisuals,
        PickupServiceConfig     config,
        DiContainer             container,
        IPickupMagnetProvider   magnetProvider,
        IPickupAcceptanceFilter acceptanceFilter,
        IPickupCarrySink        carrySink)
    {
        this.catalog          = catalog;
        this.defaultVisuals   = defaultVisuals;
        this.config           = config;
        this.container        = container;
        this.magnetProvider   = magnetProvider;
        this.acceptanceFilter = acceptanceFilter;
        this.carrySink        = carrySink;
    }

    public async UniTask SpawnAsync(PickupSpawnRequest request, CancellationToken ct = default)
    {
        if (!catalog.TryGet(request.PickupId, out _, out var overrideVisuals))
            return;

        var pool    = container.ResolveId<PickupItemViewPool>(request.PickupId);
        var visuals = overrideVisuals ?? defaultVisuals;

        for (var i = 0; i < request.Amount; i++)
        {
            await UniTask.CompletedTask;

            var view       = pool.Spawn();
            var controller = new PickupItemController(view);
            var scatterDir = request.ScatterDirection ?? GetRandomScatterDirection();

            controller.SetVisualConfig(visuals);
            controller.InitializeAsWorldItem(request.PickupId, 1, request.Position, scatterDir);
            activeWorldItems.Add(controller);
        }

        Spawned?.Invoke(new PickupSpawnedEvent(request.PickupId, request.Amount, request.Position));
    }

    public void Return(PickupItemController controller)
    {
        activeAnimatingItems.Remove(controller);
        DespawnView(controller);
    }

    public void Clear()
    {
        for (var i = activeWorldItems.Count - 1; i >= 0; i--)
            DespawnView(activeWorldItems[i]);

        activeWorldItems.Clear();

        for (var i = activeAnimatingItems.Count - 1; i >= 0; i--)
            DespawnView(activeAnimatingItems[i]);

        activeAnimatingItems.Clear();
    }

    public void Dispose()
    {
        Clear();
    }

    public void Tick()
    {
        CleanupStaleItems();
        TickActiveItemAnimations();

        if (!magnetProvider.TryGetMagnet(out var magnet))
            return;

        var radiusSqr = magnet.Radius * magnet.Radius;
        var budget    = config.CollectsPerFrame;
        var started   = 0;

        for (var i = activeWorldItems.Count - 1; i >= 0; i--)
        {
            if (started >= budget)
                break;

            var controller = activeWorldItems[i];

            if (!controller.IsWorldPickup)
            {
                activeWorldItems.RemoveAt(i);
                continue;
            }

            if (!acceptanceFilter.CanAccept(controller.PickupId, controller.Amount))
                continue;

            var delta = controller.View.Transform.position - magnet.Position;

            delta.y = 0f;

            if (delta.sqrMagnitude > radiusSqr)
                continue;

            activeWorldItems.RemoveAt(i);
            started++;
            StartCollect(controller, magnet);
        }
    }

    private void CleanupStaleItems()
    {
        for (var i = activeWorldItems.Count - 1; i >= 0; i--)
        {
            var controller = activeWorldItems[i];

            if (controller.IsRented && controller.IsWorldPickup)
                continue;

            activeWorldItems.RemoveAt(i);
        }
    }

    private void TickActiveItemAnimations()
    {
        var deltaTime = Time.deltaTime;

        for (var i = activeAnimatingItems.Count - 1; i >= 0; i--)
        {
            var controller = activeAnimatingItems[i];

            if (!controller.IsRented)
            {
                activeAnimatingItems.RemoveAt(i);
                continue;
            }

            if (controller.IsWorldPickup)
            {
                activeAnimatingItems.RemoveAt(i);
                activeWorldItems.Add(controller);
                continue;
            }

            controller.Tick(deltaTime);
        }
    }

    private void StartCollect(PickupItemController controller, PickupMagnet magnet)
    {
        var hasSink = carrySink.TryAttach(controller, out var anchor, out var lp, out var lr);

        if (hasSink)
            controller.PlayCollectAnimation(anchor, lp, lr, () => OnCollectComplete(controller, true));
        else
            controller.PlayCollectAnimation(magnet.Anchor, Vector3.zero, Quaternion.identity, () => OnCollectComplete(controller, false));

        activeAnimatingItems.Add(controller);
    }

    private void OnCollectComplete(PickupItemController controller, bool hasSink)
    {
        Collected?.Invoke(new PickupCollectedEvent(controller.PickupId, controller.Amount, controller.View.Transform.position));

        if (hasSink)
            return;

        activeAnimatingItems.Remove(controller);
        DespawnView(controller);
    }

    private void DespawnView(PickupItemController controller)
    {
        container.ResolveId<PickupItemViewPool>(controller.PickupId).Despawn(controller.View);
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
