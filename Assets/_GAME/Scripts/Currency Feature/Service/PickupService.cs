using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class PickupService : IPickupService, ITickable, IDisposable
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

        this.carrySink.Evicted += OnCarryEvicted;
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

    public void TossDeposit(string pickupId, Vector3 origin, Transform target, Action onArrived)
    {
        // Prefer melting the visible back stack so the carry pile visibly shrinks. When the player
        // carries more currency than the capped visual stack holds, fall back to a fresh pooled
        // projectile launched from the back point so the toss stream keeps flowing 1:1 with currency.
        if (carrySink.TryDetachNewest(out var carried))
        {
            carried.PlaySpendAnimation(target, () => OnDepositArrived(carried, onArrived));
            return;
        }

        if (!catalog.TryGet(pickupId, out _, out var overrideVisuals))
            return;

        var pool       = container.ResolveId<PickupItemViewPool>(pickupId);
        var view       = pool.Spawn();
        var controller = new PickupItemController(view);

        controller.SetVisualConfig(overrideVisuals ?? defaultVisuals);
        controller.InitializeAsSpendProjectile(pickupId, origin);
        controller.PlaySpendAnimation(target, () => OnDepositArrived(controller, onArrived));
        activeAnimatingItems.Add(controller);
    }

    public void Clear()
    {
        for (var i = activeWorldItems.Count - 1; i >= 0; i--)
            DespawnView(activeWorldItems[i]);

        activeWorldItems.Clear();

        for (var i = activeAnimatingItems.Count - 1; i >= 0; i--)
            DespawnView(activeAnimatingItems[i]);

        activeAnimatingItems.Clear();
        carrySink.Clear();
    }

    public void Dispose()
    {
        carrySink.Evicted -= OnCarryEvicted;
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

        // Credit immediately on collect: the collect arc and the carry stack are cosmetic, and an
        // in-flight coin can be evicted before its arc completes. Crediting here keeps currency
        // correct regardless of the visual lifetime (especially during pickup bursts).
        Collected?.Invoke(new PickupCollectedEvent(controller.PickupId, controller.Amount, controller.View.Transform.position));

        if (hasSink)
            controller.PlayCollectAnimation(anchor, lp, lr, () => OnCollectComplete(controller, true));
        else
            controller.PlayCollectAnimation(magnet.Anchor, Vector3.zero, Quaternion.identity, () => OnCollectComplete(controller, false));

        activeAnimatingItems.Add(controller);
    }

    private void OnCollectComplete(PickupItemController controller, bool hasSink)
    {
        if (hasSink)
            return;

        activeAnimatingItems.Remove(controller);
        DespawnViewAnimated(controller);
    }

    private void OnCarryEvicted(PickupItemController controller)
    {
        activeAnimatingItems.Remove(controller);
        DespawnViewAnimated(controller);
    }

    private void OnDepositArrived(PickupItemController controller, Action onArrived)
    {
        onArrived?.Invoke();

        activeAnimatingItems.Remove(controller);
        DespawnViewAnimated(controller);
    }

    private void DespawnView(PickupItemController controller)
    {
        container.ResolveId<PickupItemViewPool>(controller.PickupId).Despawn(controller.View);
    }

    private void DespawnViewAnimated(PickupItemController controller)
    {
        var view = controller.View;
        var pool = container.ResolveId<PickupItemViewPool>(controller.PickupId);

        view.PlayDespawnScale(() => pool.Despawn(view));
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
