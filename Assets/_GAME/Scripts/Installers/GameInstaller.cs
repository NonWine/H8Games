using System.Collections.Generic;
using UnityEngine;
using Zenject;

[DefaultExecutionOrder(-100)]
public class GameInstaller : MonoInstaller
{
    [SerializeField] private PlayerView heroPrefab;
    [SerializeField] private Transform ground;
    [SerializeField] private Transform heroSpawnPoint;
    [SerializeField] private Joystick joystick;
    [SerializeField] private int startingLevelIndex;
    [SerializeField] private LevelRuntime[]  levels;
    
    public override void InstallBindings()
    {
        BindSignals();
        Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle().WithArguments(levels, startingLevelIndex);

        BindUnitsModules();

        Container.BindInstance(joystick).AsSingle();
        Container.Bind<TargetReservation>().AsTransient();
        InstallHero();
    }

    private void BindUnitsModules()
    {
        Container.Bind<IUnitModulesFactory>().To<CombatUnitModulesFactory>().AsSingle().NonLazy();
        Container.Bind<IUnitModulesFactory>().To<TankUnitModulesFactory>().AsSingle().NonLazy();

        Container.Bind<ModulesFactoryCollection>().AsSingle().NonLazy();
    }

    private void BindSignals()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<StartSquadRegroupSignal>();
        Container.DeclareSignal<SquadRegroupCompletedSignal>();
        Container.DeclareSignal<SquadReachedEnemySignal>();
        Container.DeclareSignal<LoadNextLevelSignal>();
        Container.DeclareSignal<SquadDefeatedSignal>();
        Container.DeclareSignal<StartButtleSignal>();
        Container.DeclareSignal<GameIdleStateSignal>();

    }

    private void InstallHero()
    {
        Container.BindInstance(ground).WithId("Ground");
        var spawnedHero = Container.InstantiatePrefabForComponent<PlayerView>(heroPrefab, heroSpawnPoint.position, heroSpawnPoint.rotation, null);
        Container.Bind<PlayerView>().FromInstance(spawnedHero).AsSingle();
    }
}
