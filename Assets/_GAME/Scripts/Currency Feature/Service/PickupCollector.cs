using System;
using UnityEngine;
using Zenject;

public class PickupCollector : IPickupCollector, IInitializable, IDisposable
{
    private readonly IPickupRuntimeRegistry registry;
    private readonly IPickupMagnetProvider magnetProvider;
    private readonly IPickupAcceptanceFilter acceptanceFilter;
    private readonly IPickupCarrySink carrySink;
    private readonly PickupServiceConfig config;

    public event Action<PickupCollectedEvent> Collected;

    public PickupCollector(
        IPickupRuntimeRegistry  registry,
        IPickupMagnetProvider   magnetProvider,
        IPickupAcceptanceFilter acceptanceFilter,
        IPickupCarrySink        carrySink,
        PickupServiceConfig     config)
    {
        this.registry         = registry;
        this.magnetProvider   = magnetProvider;
        this.acceptanceFilter = acceptanceFilter;
        this.carrySink        = carrySink;
        this.config           = config;
    }

    public void Initialize()
    {
        carrySink.Evicted += OnCarryEvicted;
    }

    public void Dispose()
    {
        carrySink.Evicted -= OnCarryEvicted;
    }

    public void Tick()
    {
        if (!magnetProvider.TryGetMagnet(out var magnet))
            return;

        var radiusSqr = magnet.Radius * magnet.Radius;
        var budget    = config.CollectsPerFrame;
        var started   = 0;

        for (var i = registry.WorldItems.Count - 1; i >= 0; i--)
        {
            if (started >= budget)
                break;

            var controller = registry.WorldItems[i];

            if (!controller.IsRented || !controller.IsWorldPickup)
            {
                registry.RemoveWorldAt(i);
                continue;
            }

            if (!acceptanceFilter.CanAccept(controller.PickupId, controller.Amount))
                continue;

            var delta = controller.View.Transform.position - magnet.Position;

            delta.y = 0f;

            if (delta.sqrMagnitude > radiusSqr)
                continue;

            registry.PromoteToAnimating(i);
            started++;
            StartCollect(controller, magnet);
        }
    }

    private void StartCollect(PickupItemController controller, PickupMagnet magnet)
    {
        var hasSink = carrySink.TryAttach(controller, out var anchor, out var lp, out var lr);
        Collected?.Invoke(new PickupCollectedEvent(controller.PickupId, controller.Amount, controller.View.Transform.position));

        if (hasSink)
            controller.PlayCollectAnimation(anchor, lp, lr, () => OnCollectComplete(controller, true));
        else
            controller.PlayCollectAnimation(magnet.Anchor, Vector3.zero, Quaternion.identity, () => OnCollectComplete(controller, false));
    }

    private void OnCollectComplete(PickupItemController controller, bool hasSink)
    {
        if (hasSink)
            return;

        registry.RemoveAnimating(controller);
        registry.DespawnAnimated(controller);
    }

    private void OnCarryEvicted(PickupItemController controller)
    {
        registry.RemoveAnimating(controller);
        registry.DespawnAnimated(controller);
    }
}
