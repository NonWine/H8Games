using UnityEngine;
using Zenject;

public class HitStopInstaller : MonoInstaller
{
    [SerializeField] private HitStopConfig config;

    public override void InstallBindings()
    {
        Container.Bind<HitStopConfig>().FromInstance(config).AsSingle();
        Container.BindInterfacesTo<HitStopService>().AsSingle();
        Container.BindInterfacesTo<CombatHitStopListener>().AsSingle().NonLazy();
    }
}
