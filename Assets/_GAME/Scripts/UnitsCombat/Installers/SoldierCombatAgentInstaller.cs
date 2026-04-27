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
                CombatView.unitConfig.AuthoringStats.ReservationPenalty,
                CombatView.unitConfig.AuthoringStats.DetectionRadius);
        Container.Bind<ICombatTargetValidator>()
            .To<EnemyGroupCombatTargetValidator>()
            .AsSingle();
    }

    private void BindRuntime()
    {
        Container.Bind<SoldierRuntimeModel>().AsSingle();
        Container.Bind<AgentRuntimeModel>()
            .FromMethod(context => context.Container.Resolve<SoldierRuntimeModel>())
            .AsSingle();
        Container.Bind<IAliveState>()
            .FromMethod(context => context.Container.Resolve<AgentRuntimeModel>())
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
        Container.BindInterfacesAndSelfTo<SoldierCombatAgentController>().AsSingle();
    }
}
