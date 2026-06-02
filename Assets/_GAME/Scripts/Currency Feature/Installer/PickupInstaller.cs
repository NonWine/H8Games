using UnityEngine;
using Zenject;

public class PickupInstaller : MonoInstaller
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

        Container.BindFactory<PickupItemView, PickupVisualConfig, PickupItemController, PickupItemController.Factory>();

        Container.Bind<IPickupAcceptanceFilter>().To<NullPickupAcceptanceFilter>().AsSingle();
        Container.Bind<IPickupCarrySink>().To<StackPickupCarrySink>().AsSingle();

        Container.Bind<PickupViewPools>().AsSingle();
        Container.BindInterfacesAndSelfTo<PickupRuntimeRegistry>().AsSingle();
        Container.BindInterfacesAndSelfTo<PickupSpawner>().AsSingle();
        Container.BindInterfacesAndSelfTo<PickupDepositer>().AsSingle();
        Container.BindInterfacesAndSelfTo<PickupCollector>().AsSingle();
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
