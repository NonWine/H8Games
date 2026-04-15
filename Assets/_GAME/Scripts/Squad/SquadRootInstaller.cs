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
        Container.Bind<FormationLayoutService>().AsSingle();
        Container.Bind<SquadFormationRegistry>().AsSingle();

        Container.Bind<SquadFormationController>()
            .AsSingle()
            .WithArguments(
                squadRootViewAnchor,
                squadRootViewAnchor.transform,
                squadRootViewAnchor.InitialCapacity);


        Container.BindInterfacesAndSelfTo<SquadRootIdleState>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadMoveToEnemyState>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadReturnGroupState>().AsSingle();
        Container.Bind<SquadRootStateMachine>().AsSingle();

        Container.BindInterfacesAndSelfTo<SquadMoveProvider>().AsSingle().WithArguments(squadRootViewAnchor.transform, squadFollowSettings, squadRootViewAnchor.TargetReachThreshold);
        Container.BindInterfacesAndSelfTo<SquadMovementFacade>().AsSingle();
        Container.BindInterfacesAndSelfTo<SquadFormationFacade>().AsSingle();
    }
}
