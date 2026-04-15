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
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<ClearedLastEnemyGroup>();
        Container.DeclareSignal<StartSquadRegroupSignal>();
        Container.DeclareSignal<SquadRegroupCompletedSignal>();
        Container.DeclareSignal<LoadNextLevelSignal>();

        Container.BindInstance(joystick).AsSingle();
        Container.Bind<TargetReservation>().AsTransient();
        InstallHero();
    }
    private void InstallHero()
    {
        var spawnedHero = Container.InstantiatePrefabForComponent<PlayerView>(heroPrefab, heroSpawnPoint.position, heroSpawnPoint.rotation, null);
        Container.Bind<PlayerView>().FromInstance(spawnedHero).AsSingle();
    }
}
