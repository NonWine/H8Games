using UnityEngine;
using Zenject;

public class EnemyCombatAgentInstaller : CombatAgentInstaller
{
    [SerializeField] private EnemySortieConfig sortieConfig;

    protected override void BindModules()
    {
        base.BindModules();

        Container.Rebind<IDeathModule>().To<EnemyDeathModule>()
            .AsSingle()
            .WithArguments(
                CombatView.gameObject,
                CombatView.unitConfig.AuthoringStats.DeathPickupId,
                CombatView.unitConfig.AuthoringStats.DeathReward);
    }

    protected override void InstallFeatureBindings()
    {
        BindTargeting();
        BindRuntime();
        BindSortie();
        BindStateMachine();
        BindController();
    }

    private void BindTargeting()
    {
        Container.Bind<ICombatTargetProvider>()
            .To<AllyCombatTargetProvider>()
            .AsSingle();
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

    private void BindSortie()
    {
        Container.BindInstance(sortieConfig).AsSingle();
        Container.Bind<EnemySortiePlanner>().AsSingle();
        Container.Bind<EnemyNavMeshMover>().AsSingle();
    }

    private void BindStateMachine()
    {
        Container.Bind<EnemyStateBase>().To<EnemyIdleState>().AsSingle();
        Container.Bind<EnemyStateBase>().To<EnemyAttackState>().AsSingle();
        Container.Bind<EnemyStateBase>().To<EnemySortieState>().AsSingle();
        Container.Bind<EnemyStateBase>().To<EnemyReturnState>().AsSingle();
        Container.Bind<EnemyStateBase>().To<EnemyDeadState>().AsSingle();
        Container.Bind<EnemyStateMachine>().AsSingle();
    }

    private void BindController()
    {
        Container.BindInterfacesAndSelfTo<EnemyCombatAgentController>().AsSingle();
    }
}
