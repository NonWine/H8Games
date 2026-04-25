using Zenject;

public class SoldierCombatAgentInstaller : CombatAgentInstaller
{
    protected override void InstallFeatureBindings()
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
        Container.Bind<SoldierRuntimeModel>().AsSingle();
        Container.Bind<SoldierMovingFormationService>()
            .FromMethod(context => new SoldierMovingFormationService(context.Container.Resolve<SquadFollowSettings>(), CombatView.GetInstanceID()))
            .AsSingle();
        Container.Bind<SoldierStateBase>().To<SoldierIdleState>().AsSingle();
        Container.Bind<SoldierStateBase>().To<SoldierMoveState>().AsSingle();
        Container.Bind<SoldierStateBase>().To<SoldierAttackState>().AsSingle();
        Container.Bind<SoldierStateBase>().To<SoldierDeadState>().AsSingle();
        Container.Bind<SoldierStateMachine>().AsSingle();
        Container.BindInterfacesAndSelfTo<SoldierCombatAgentController>().AsSingle().NonLazy();
    }
}
