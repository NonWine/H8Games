using Zenject;

public class EnemyCombatAgentInstaller : CombatAgentInstaller
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
            .To<AllyCombatTargetProvider>()
            .AsSingle()
            .WithArguments(
                CombatView.transform,
                CombatView.unitConfig.AuthoringStats.ReservationPenalty);
    }

    private void BindRuntime()
    {
        Container.Bind<AgentRuntimeModel>().To<EnemyRuntimeModel>().AsSingle();
        Container.Bind<EnemyRuntimeModel>().AsSingle();
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
        Container.BindInterfacesAndSelfTo<EnemyCombatAgentController>().AsSingle().NonLazy();
    }
}
