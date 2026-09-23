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
    [SerializeField] private LevelRuntime[] levels;

    public override void InstallBindings()
    {
        BindSignals();
        Container.BindInterfacesTo<PickupCurrencyBridge>().AsSingle().NonLazy();
        Container.BindInterfacesTo<PickupDefeatBridge>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle().WithArguments(levels, startingLevelIndex);
        Container.BindInterfacesAndSelfTo<LevelProgressTracker>().AsSingle();
        Container.BindInstance(joystick).AsSingle();
        Container.Bind<TargetReservationHandler>().AsTransient();

        // Shared by the hero and every soldier: each unit picks its own target,
        // but they all read the same once-per-frame sweep of the level's enemies.
        Container.Bind<IEnemyCandidateSource>().To<LevelEnemyCandidateSource>().AsSingle();
        InstallHero();
    }

    private void BindSignals()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<StartSquadRegroupSignal>();
        Container.DeclareSignal<SquadRegroupCompletedSignal>();
        Container.DeclareSignal<SquadReachedEnemySignal>();
        Container.DeclareSignal<LoadNextLevelSignal>();
        Container.DeclareSignal<LevelCompletedSignal>();
        Container.DeclareSignal<LevelCaptureCompletedSignal>();
        Container.DeclareSignal<SquadDefeatedSignal>();
        Container.DeclareSignal<HeroDefeatedSignal>();
        Container.DeclareSignal<HeroDamagedSignal>();
        Container.DeclareSignal<StartButtleSignal>();
        Container.DeclareSignal<GameIdleStateSignal>();
        Container.DeclareSignal<LevelRestartedSignal>();

        // Per-unit combat feedback. Fired from inside each agent's own
        // sub-container and consumed at scene scope by audio, shake and
        // hit-stop, none of which the units know about.
        Container.DeclareSignal<UnitDamagedSignal>();
        Container.DeclareSignal<UnitDiedSignal>();

    }

    // The hero prefab carries a GameObjectContext that resolves its own combat
    // graph the moment it is instantiated, and that graph needs scene-scope
    // services such as LevelManager. Spawning during InstallBindings therefore ran
    // ahead of the installers that bind them and aborted the whole scene install,
    // so every binding here is deferred to first resolve instead.
    private void InstallHero()
    {
        Container.BindInstance(ground).WithId("Ground").AsCached();

        Container.Bind<PlayerView>().FromMethod(SpawnHero).AsSingle().NonLazy();

        Container.Bind<IPickupCarryAnchorProvider>()
            .FromMethod(context => context.Container.Resolve<PlayerView>())
            .AsSingle();

        Container.Bind<IPickupMagnetProvider>()
            .FromMethod(context => context.Container.Resolve<PlayerView>().GetComponentInChildren<IPickupMagnetProvider>())
            .AsSingle();

        // Re-bound at scene scope, same as PlayerView above, so enemy
        // subcontainers can resolve the hero as a targetable candidate
        // (see AllyCombatTargetProvider) the same way they already resolve
        // SquadFormationRegistry from squad scope.
        Container.Bind<HeroCombatAgentController>()
            .FromMethod(context => context.Container.Resolve<PlayerView>()
                .GetComponent<GameObjectContext>()
                .Container.Resolve<HeroCombatAgentController>())
            .AsSingle();
    }

    private PlayerView SpawnHero(InjectContext context)
    {
        return context.Container.InstantiatePrefabForComponent<PlayerView>(
            heroPrefab,
            heroSpawnPoint.position,
            heroSpawnPoint.rotation,
            null);
    }
}
