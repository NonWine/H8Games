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
        Container.Bind<ICombatTargetProvider>().To<EnemyGroupCombatTargetProvider>().AsSingle();
        Container.Rebind<ICombatTargetValidator>().To<EnemyGroupCombatTargetValidator>().AsSingle();
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
        Container.Bind<ISoldierFormationMover>()
            .FromMethod(context => new SoldierNavMeshFormationMover(
                context.Container.Resolve<BaseCombatAgentView>(),
                context.Container.Resolve<SquadFollowSettings>(),
                CombatView.GetInstanceID()))
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
        Container.BindInterfacesAndSelfTo<SoldierPoolableRoot>().FromComponentOnRoot().AsSingle();
    }
}
