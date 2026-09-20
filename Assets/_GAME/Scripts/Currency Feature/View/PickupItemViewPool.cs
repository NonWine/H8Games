using Zenject;

public class PickupItemViewPool : MemoryPool<PickupItemView>
{
    protected override void OnSpawned(PickupItemView item)
    {
        item.gameObject.SetActive(true);
        item.Rent();
    }

    protected override void OnDespawned(PickupItemView item)
    {
        // Scene/context teardown can destroy pooled items before Zenject's
        // DisposableManager reaches this despawn call; skip cleanup on an
        // already-destroyed item instead of touching its dangling Transform.
        if (item == null)
            return;

        item.Cleanup();
        item.gameObject.SetActive(false);
    }
}
