using UnityEngine;
using Zenject;

public sealed class PickupInstaller : MonoInstaller
{
    [SerializeField] private PickupCatalog       catalog;
    [SerializeField] private PickupVisualConfig  defaultVisuals;
    [SerializeField] private PickupServiceConfig serviceConfig = new();

    public override void InstallBindings()
    {
        Container.BindInstance(catalog).AsSingle();
        Container.BindInstance(defaultVisuals).AsSingle();
        Container.BindInstance(serviceConfig).AsSingle();

        BindPools();

        Container.Bind<IPickupAcceptanceFilter>().To<NullPickupAcceptanceFilter>().AsSingle();
        Container.Bind<IPickupCarrySink>().To<NullPickupCarrySink>().AsSingle();

        Container.BindInterfacesAndSelfTo<PickupService>().AsSingle();
    }

    private void BindPools()
    {
        catalog.ForEachEntry((pickupId, prefab, _) =>
        {
            Container.BindMemoryPool<PickupItemView, PickupItemViewPool>()
                .WithId(pickupId)
                .WithInitialSize(3)
                .FromComponentInNewPrefab(prefab)
                .UnderTransformGroup("Pickups");
        });
    }
}
