using UnityEngine;
using Zenject;

public class SquadCombatInstaller : MonoInstaller
{
    [SerializeField] private int startingLevelIndex;
    [SerializeField] private GamePhase initialPhase = GamePhase.Preparation;

    public override void InstallBindings()
    {
        LevelRuntime[] levels = FindObjectsByType<LevelRuntime>(FindObjectsSortMode.None);

        Container.BindInstance(new GamePhaseService(initialPhase)).AsSingle();
        Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle().WithArguments(levels, startingLevelIndex);

        Container.Bind<SquadSoldierRegistry>().AsSingle();
        Container.Bind<SquadEncounterController>().AsSingle();
        Container.Bind<SquadDefeatWatcher>().AsSingle();
        Container.Bind<EnemyGroupDetector>().AsSingle();
        Container.Bind<SquadAllyTargetSelector>().AsSingle();
        Container.BindInterfacesAndSelfTo<CombatStateController>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadCombatCoordinator>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadReturnGroupState>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadCombatPresenter>().AsSingle();
    }
}
