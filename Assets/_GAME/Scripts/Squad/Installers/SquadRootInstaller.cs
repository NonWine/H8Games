using UnityEngine;
using Zenject;

public class SquadRootInstaller : MonoInstaller
{
    [SerializeField] private SquadRootView squadRootViewAnchor;
    [SerializeField] private SquadFollowSettings squadFollowSettings;

    public override void InstallBindings()
    {
        Container.BindInstance(squadRootViewAnchor).AsSingle();
        Container.BindInstance(squadFollowSettings).AsSingle();
        Services();
        SquadStateMachine();
        Controllers();
    }

    private void Controllers()
    {
        Container.BindInterfacesAndSelfTo<SquadCombatStateController>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadMoveProvider>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadMovementFacade>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadFormationFacade>().AsSingle();
    }

    private void Services()
    {
        Container.Bind<SquadFormationLayoutService>().AsSingle();
        Container.Bind<SquadFormationRegistry>().AsSingle();
        Container.Bind<SquadFormationController>().AsSingle();
        Container.Bind<EnemyGroupDetector>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadAllyTargetSelector>().AsSingle();
        Container.Bind<EnemyDestinationContex>().AsSingle();
        Container.Bind<IDestinationProvider>().To<EnemyDestinationContex>().FromResolve();
    }

    private void SquadStateMachine()
    {
        Container.BindInterfacesAndSelfTo<SquadRootIdleState>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadMoveToEnemyState>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadReturnGroupState>().AsSingle();
        Container.Bind<SquadRootStateMachine>().AsSingle();
    }
}
