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
        Container.BindInstance(joystick).AsSingle();
        InstallHero();
        Container.Bind<TargetReservation>().AsTransient();
    }
    private void InstallHero()
    {
        var spawnedHero = Container.InstantiatePrefabForComponent<PlayerView>(heroPrefab, heroSpawnPoint.position, heroSpawnPoint.rotation, null);
        Container.Bind<PlayerView>().FromInstance(spawnedHero).AsSingle();
    }
}
