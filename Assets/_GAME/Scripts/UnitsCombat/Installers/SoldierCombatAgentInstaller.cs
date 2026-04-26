using Zenject;

public class SoldierCombatAgentInstaller : CombatAgentInstaller
{
    protected override void InstallFeatureBindings()
    {
        BindTargeting();
        BindRuntime();
        BindStateMachine();
        BindController();
    }

    private void BindTargeting()
    {
        Container.Bind<ICombatTargetProvider>()
            .To<EnemyGroupCombatTargetProvider>()
            .AsSingle()
            .WithArguments(
                CombatView.transform,
                CombatView.unitConfig.AuthoringStats.ReservationPenalty);
        Container.Bind<ICombatTargetValidator>()
            .To<EnemyGroupCombatTargetValidator>()
            .AsSingle();
    }

    private void BindRuntime()
    {
        Container.Bind<SoldierRuntimeModel>().AsSingle();
        Container.Bind<AgentRuntimeModel>()
            .FromResolveGetter<SoldierRuntimeModel>(x => x)
            .AsSingle();
        Container.Bind<SoldierMovingFormationService>()
            .FromMethod(context => new SoldierMovingFormationService(context.Container.Resolve<SquadFollowSettings>(), CombatView.GetInstanceID()))
            .AsSingle();
    }

    private void BindStateMachine()
    {
        Container.Bind<SoldierStateBase>().To<SoldierIdleState>().AsSingle();
        Container.Bind<SoldierStateBase>().To<SoldierMoveState>().AsSingle();
        Container.Bind<SoldierStateBase>().To<SoldierAttackState>().AsSingle();
        Container.Bind<SoldierStateBase>().To<SoldierDeadState>().AsSingle();
        Container.Bind<SoldierStateMachine>().AsSingle();
    }

    private void BindController()
    {
        Container.BindInterfacesAndSelfTo<SoldierCombatAgentController>().AsSingle().NonLazy();
    }
}
