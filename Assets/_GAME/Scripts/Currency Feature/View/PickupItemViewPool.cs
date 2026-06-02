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
        item.Cleanup();
        item.gameObject.SetActive(false);
    }
}
