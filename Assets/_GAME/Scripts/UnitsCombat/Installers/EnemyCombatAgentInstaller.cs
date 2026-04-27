using Zenject;

public class EnemyCombatAgentInstaller : CombatAgentInstaller
{
    protected override void BindModules()
    {
        base.BindModules();

        Container.Rebind<IDeathModule>().To<EnemyDeathModule>()
            .AsSingle()
            .WithArguments(
                CombatView.gameObject,
                CombatView.unitConfig.AuthoringStats.DeathReward);
    }

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
            .To<AllyCombatTargetProvider>()
            .AsSingle()
            .WithArguments(
                CombatView.transform,
                CombatView.unitConfig.AuthoringStats.ReservationPenalty);
    }

    private void BindRuntime()
    {
        Container.Bind<EnemyRuntimeModel>().AsSingle();
        Container.Bind<AgentRuntimeModel>()
            .FromMethod(context => context.Container.Resolve<EnemyRuntimeModel>())
            .AsSingle();
        Container.Bind<IAliveState>()
            .FromMethod(context => context.Container.Resolve<AgentRuntimeModel>())
            .AsSingle();
    }

    private void BindStateMachine()
    {
        Container.Bind<EnemyStateBase>().To<EnemyIdleState>().AsSingle();
        Container.Bind<EnemyStateBase>().To<EnemyAttackState>().AsSingle();
        Container.Bind<EnemyStateBase>().To<EnemyDeadState>().AsSingle();
        Container.Bind<EnemyStateMachine>().AsSingle();
    }

    private void BindController()
    {
        Container.BindInterfacesAndSelfTo<EnemyCombatAgentController>().AsSingle();
    }
}
