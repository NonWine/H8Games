using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    private PlayerView heroView;

    public override void InstallBindings()
    {
        heroView = GetComponent<PlayerView>();
        
        Container.Bind<PlayerView>().FromInstance(heroView).AsSingle();
        Container.BindInterfacesAndSelfTo<HeroCombatRuntime>().AsSingle();
        Container.Bind<IHeroInputReader>().To<HeroJoystickInputReader>().AsSingle();
        Container.Bind<IHeroMover>().To<HeroKinematicMover>().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle();
    }
}
