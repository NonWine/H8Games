using UnityEngine;
using Zenject;

public class SquadCombatInstaller : MonoInstaller
{
    [SerializeField] private int startingLevelIndex;
    [SerializeField] private GamePhase initialPhase = GamePhase.Preparation;
    [SerializeField] private LevelRuntime[]  levels;
    public override void InstallBindings()
    {

        Container.BindInstance(new GamePhaseService(initialPhase)).AsSingle();
        Container.BindInterfacesAndSelfTo<LevelManager>().AsSingle().WithArguments(levels, startingLevelIndex);

        Container.Bind<EnemyGroupDetector>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadAllyTargetSelector>().AsSingle();
        Container.Bind<EnemyDestinationContex>().AsSingle();
        Container.Bind<IDestinationProvider>().To<EnemyDestinationContex>().FromResolve();
        Container.BindInterfacesAndSelfTo<CombatStateController>().AsSingle();
    }
}
