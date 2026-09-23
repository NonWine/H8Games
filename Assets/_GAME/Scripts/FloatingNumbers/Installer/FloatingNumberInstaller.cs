using UnityEngine;
using Zenject;

public class FloatingNumberInstaller : MonoInstaller
{
    [SerializeField] private FloatingNumberView prefab;
    [SerializeField] private FloatingNumberConfig config;

    public override void InstallBindings()
    {
        Container.BindInstance(config).AsSingle();
        Container.BindMemoryPool<FloatingNumberView, FloatingNumberPool>()
            .WithInitialSize(16)
            .FromComponentInNewPrefab(prefab)
            .UnderTransformGroup("FloatingNumbers");
        Container.BindInterfacesTo<FloatingNumberPresenter>().AsSingle().NonLazy();
    }
}
