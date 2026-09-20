using UnityEngine;
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

    // Each soldier picks the nearest enemy inside his own DetectionRadius rather
    // than sharing one squad-wide target group. The squad still decides where the
    // formation marches - it just no longer decides who each man shoots.
    //
    // Sensing is gated on the encounter phase all the same: between fights a man
    // walking home to his slot must not lock onto a bystander and stall there.
    private void BindTargeting()
    {
        Container.Bind<ICombatTargetProvider>()
            .FromMethod(context => new CombatPhaseGatedTargetProvider(
                new NearestEnemyCombatTargetProvider(
                    context.Container.Resolve<Transform>(),
                    context.Container.Resolve<TargetingData>(),
                    context.Container.Resolve<IEnemyCandidateSource>()),
                context.Container.Resolve<ICombatStateProvider>()))
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
