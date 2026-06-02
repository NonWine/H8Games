using Zenject;

public class PickupViewPools
{
    private readonly DiContainer container;

    public PickupViewPools(DiContainer container)
    {
        this.container = container;
    }

    public PickupItemView Spawn(string pickupId)
    {
        return container.ResolveId<PickupItemViewPool>(pickupId).Spawn();
    }

    public void Despawn(string pickupId, PickupItemView view)
    {
        container.ResolveId<PickupItemViewPool>(pickupId).Despawn(view);
    }
}
