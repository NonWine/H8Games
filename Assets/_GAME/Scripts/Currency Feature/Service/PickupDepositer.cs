using System;
using UnityEngine;

public class PickupDepositer : IPickupDepositer
{
    private readonly PickupCatalog catalog;
    private readonly PickupVisualConfig defaultVisuals;
    private readonly PickupViewPools pools;
    private readonly PickupItemController.Factory controllerFactory;
    private readonly IPickupRuntimeRegistry registry;
    private readonly IPickupCarrySink carrySink;

    public PickupDepositer(
        PickupCatalog                catalog,
        PickupVisualConfig           defaultVisuals,
        PickupViewPools              pools,
        PickupItemController.Factory controllerFactory,
        IPickupRuntimeRegistry       registry,
        IPickupCarrySink             carrySink)
    {
        this.catalog           = catalog;
        this.defaultVisuals    = defaultVisuals;
        this.pools             = pools;
        this.controllerFactory = controllerFactory;
        this.registry          = registry;
        this.carrySink         = carrySink;
    }

    public void TossDeposit(string pickupId, Vector3 origin, Transform target, Action onArrived)
    {
        if (carrySink.TryDetachNewest(out var carried))
        {
            carried.PlaySpendAnimation(target, () => OnDepositArrived(carried, onArrived));
            return;
        }

        if (!catalog.TryGet(pickupId, out _, out var overrideVisuals))
            return;

        var view       = pools.Spawn(pickupId);
        var controller = controllerFactory.Create(view, overrideVisuals ?? defaultVisuals);

        controller.InitializeAsSpendProjectile(pickupId, origin);
        controller.PlaySpendAnimation(target, () => OnDepositArrived(controller, onArrived));
        registry.AddAnimating(controller);
    }

    private void OnDepositArrived(PickupItemController controller, Action onArrived)
    {
        onArrived?.Invoke();

        registry.RemoveAnimating(controller);
        registry.DespawnAnimated(controller);
    }
}
