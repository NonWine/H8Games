using Zenject;

public class EnemyCombatAgentInstaller : CombatAgentInstaller
{
    protected override void InstallFeatureBindings()
    {
        Container.Bind<ICombatTargetProvider>()
            .To<AllyCombatTargetProvider>()
            .AsSingle()
            .WithArguments(
                CombatView.transform,
                CombatView.unitConfig.AuthoringStats.ReservationPenalty);
        Container.Bind<EnemyRuntimeModel>().AsSingle();
        Container.Bind<EnemyStateBase>().To<EnemyIdleState>().AsSingle();
        Container.Bind<EnemyStateBase>().To<EnemyAttackState>().AsSingle();
        Container.Bind<EnemyStateBase>().To<EnemyDeadState>().AsSingle();
        Container.Bind<EnemyStateMachine>().AsSingle();
        Container.BindInterfacesAndSelfTo<EnemyCombatAgentController>().AsSingle().NonLazy();
    }
}
