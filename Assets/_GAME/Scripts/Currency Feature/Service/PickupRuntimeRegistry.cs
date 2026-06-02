using System.Collections.Generic;

public class PickupRuntimeRegistry : IPickupRuntimeRegistry
{
    private readonly PickupViewPools pools;
    private readonly List<PickupItemController> worldItems = new();
    private readonly List<PickupItemController> animatingItems = new();

    public IReadOnlyList<PickupItemController> WorldItems => worldItems;

    public PickupRuntimeRegistry(PickupViewPools pools)
    {
        this.pools = pools;
    }

    public void AddWorld(PickupItemController controller)
    {
        worldItems.Add(controller);
    }

    public void AddAnimating(PickupItemController controller)
    {
        animatingItems.Add(controller);
    }

    public void RemoveAnimating(PickupItemController controller)
    {
        animatingItems.Remove(controller);
    }

    public void RemoveWorldAt(int index)
    {
        worldItems.RemoveAt(index);
    }

    public void PromoteToAnimating(int worldIndex)
    {
        var controller = worldItems[worldIndex];
        worldItems.RemoveAt(worldIndex);
        animatingItems.Add(controller);
    }

    public void TickAnimations(float deltaTime)
    {
        for (var i = animatingItems.Count - 1; i >= 0; i--)
        {
            var controller = animatingItems[i];

            if (!controller.IsRented)
            {
                animatingItems.RemoveAt(i);
                continue;
            }

            if (controller.IsWorldPickup)
            {
                animatingItems.RemoveAt(i);
                worldItems.Add(controller);
                continue;
            }

            controller.Tick(deltaTime);
        }
    }

    public void Despawn(PickupItemController controller)
    {
        pools.Despawn(controller.PickupId, controller.View);
    }

    public void DespawnAnimated(PickupItemController controller)
    {
        var pickupId = controller.PickupId;
        var view     = controller.View;

        view.PlayDespawnScale(() => pools.Despawn(pickupId, view));
    }

    public void Clear()
    {
        for (var i = worldItems.Count - 1; i >= 0; i--)
            Despawn(worldItems[i]);

        worldItems.Clear();

        for (var i = animatingItems.Count - 1; i >= 0; i--)
            Despawn(animatingItems[i]);

        animatingItems.Clear();
    }
}
