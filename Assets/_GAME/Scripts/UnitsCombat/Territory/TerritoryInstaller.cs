using UnityEngine;
using Zenject;

public class TerritoryInstaller : MonoInstaller
{
    [SerializeField] private TerritoryView          view;
    [SerializeField] private TerritoryConfig         config;
    [SerializeField] private TerritoryDangerCameraFX cameraFX;

    [Header("Flag Anchor")]
    [Tooltip("Drag the flag/objective Transform here. The territory zone will always include this point " +
             "and shrink to a circle around it when all enemies are dead.")]
    [SerializeField] private Transform flagAnchor;

    public override void InstallBindings()
    {
        BindViews();
        BindData();
        BindServices();
    }

    private void BindViews()
    {
        Container.BindInstance(view).AsSingle();
        Container.Bind<ITerritoryView>().To<TerritoryView>().FromResolve();
        Container.BindInstance(cameraFX).AsSingle();
    }

    private void BindData()
    {
        Container.BindInstance(config).AsSingle();
        Container.Bind<TerritoryMeshBuilder>().AsSingle();
    }

    private void BindServices()
    {
        if (flagAnchor != null)
            Container.BindInstance(flagAnchor).WithId("TerritoryFlagAnchor").AsSingle();

        Container.BindInterfacesAndSelfTo<TerritoryService>().AsSingle();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (view == null)
            Debug.LogWarning("[TerritoryInstaller] TerritoryView is not assigned.", this);

        if (config == null)
            Debug.LogWarning("[TerritoryInstaller] TerritoryConfig is not assigned.", this);

        if (cameraFX == null)
            Debug.LogWarning("[TerritoryInstaller] TerritoryDangerCameraFX is not assigned.", this);
    }
#endif
}
