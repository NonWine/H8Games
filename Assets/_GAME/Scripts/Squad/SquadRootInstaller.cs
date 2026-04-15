using UnityEngine;
using Zenject;

public class SquadRootInstaller : MonoInstaller
{
    [SerializeField] private SquadRoot squadRootAnchor;
    [SerializeField] private SquadFollowSettings squadFollowSettings;

    public override void InstallBindings()
    {
        Container.BindInstance(squadFollowSettings).AsSingle();
        Container.Bind<FormationLayoutService>().AsSingle();
        

        Container.Bind<SquadFormationController>()
            .AsSingle()
            .WithArguments(
                squadRootAnchor.transform,
                squadRootAnchor.InitialCapacity);

        Container.Bind<SquadRootStateMachine>().AsSingle();
        Container.Bind<SquadRootIdleState>().AsSingle();

        Container.Bind<SquadMoveProvider>()
            .AsSingle()
            .WithArguments(
                squadRootAnchor.transform,
                squadFollowSettings,
                squadRootAnchor.TargetReachThreshold);

        Container.BindInterfacesAndSelfTo<SquadMovementFacade>().AsSingle();
        Container.Bind<SquadFormationFacade>().AsSingle();
    }
}