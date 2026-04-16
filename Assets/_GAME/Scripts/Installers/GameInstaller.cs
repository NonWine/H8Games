using System.Collections.Generic;
using UnityEngine;
using Zenject;

[DefaultExecutionOrder(-100)]
public class GameInstaller : MonoInstaller
{
    [SerializeField] private PlayerView heroPrefab;
    [SerializeField] private Transform heroSpawnPoint;
    [SerializeField] private Joystick joystick;
  
    public override void InstallBindings()
    {
        BindSignals();

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
        Container.DeclareSignal<ClearedLastEnemyGroup>();
        Container.DeclareSignal<StartSquadRegroupSignal>();
        Container.DeclareSignal<SquadRegroupCompletedSignal>();
        Container.DeclareSignal<SquadReachedEnemySignal>();
        Container.DeclareSignal<LoadNextLevelSignal>();
        Container.DeclareSignal<SquadDefeatedSignal>();
    }

    private void InstallHero()
    {
        var spawnedHero = Container.InstantiatePrefabForComponent<PlayerView>(heroPrefab, heroSpawnPoint.position, heroSpawnPoint.rotation, null);
        Container.Bind<PlayerView>().FromInstance(spawnedHero).AsSingle();
    }
}
