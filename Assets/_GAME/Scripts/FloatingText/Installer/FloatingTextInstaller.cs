using UnityEngine;
using Zenject;

public class FloatingTextInstaller : MonoInstaller
{
    [SerializeField] private FloatingTextConfig config;
    [SerializeField] private FloatingTextPool pool;
    [SerializeField] private Transform cameraTransform;

    public override void InstallBindings()
    {
        BindData();
        BindServices();
    }

    private void BindData()
    {
        Container.Bind<FloatingTextConfig>().FromInstance(config).AsSingle();
        Container.Bind<FloatingTextPool>().FromInstance(pool).AsSingle();
    }

    private void BindServices()
    {
        Container.BindInterfacesTo<FloatingTextService>()
            .AsSingle()
            .WithArguments(cameraTransform);

        Container.BindInterfacesTo<CombatFloatingTextListener>().AsSingle().NonLazy();
    }
}
