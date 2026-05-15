using UnityEngine;
using Zenject;

public class TerritoryInstaller : MonoInstaller
{
    [SerializeField] private TerritoryView   view;
    [SerializeField] private TerritoryConfig config;

    public override void InstallBindings()
    {
        Container.BindInstance(config).AsSingle();
        Container.Bind<TerritoryMeshBuilder>().AsSingle();
        Container.BindInstance(view).AsSingle();
        Container.Bind<ITerritoryView>().To<TerritoryView>().FromResolve();
        Container.BindInterfacesTo<TerritoryService>().AsSingle();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (view == null)
            Debug.LogWarning("[TerritoryInstaller] TerritoryView is not assigned.", this);

        if (config == null)
            Debug.LogWarning("[TerritoryInstaller] TerritoryConfig is not assigned.", this);
    }
#endif
}
