using UnityEngine;
using Zenject;

public class  CombatAgentInstaller : MonoInstaller
{
    [SerializeField] private BaseCombatAgentView combatView;
    protected BaseCombatAgentView CombatView => combatView;

    public override void InstallBindings()
    {
        UnitStats unitStats = combatView.unitConfig.CreateRuntimeStats();

        Container.Bind<BaseCombatUnitView>().FromInstance(combatView).AsSingle();
        Container.Bind<BaseCombatAgentView>().FromInstance(combatView).AsSingle();
        Container.Bind<UnitStats>().FromInstance(unitStats).AsSingle();
        Container.Bind<CombatUnitModules>()
            .FromMethod(context =>
            {
                ModulesFactoryCollection modulesFactoryCollection = context.Container.Resolve<ModulesFactoryCollection>();
                IUnitModulesFactory unitModuleFactory = modulesFactoryCollection.Create(combatView.unitConfig.unitModuleType);
                return unitModuleFactory.Create(new CombatUnitModulesArgs(combatView, unitStats));
            })
            .AsSingle();
        Container.Bind<ICombatTargetValidator>().To<DefaultCombatTargetValidator>().AsSingle();
        Container.Bind<ITargetReservationHandler>().To<TargetReservationHandler>().AsSingle();
        Container.Bind<ITargetTrackerHandler>()
            .To<CombatTargetTracker>()
            .AsSingle()
            .WithArguments(
                combatView.unitConfig.AuthoringStats.RetargetInterval,
                combatView.unitConfig.AuthoringStats.TargetLockDuration);
        Container.Bind<UnitRotatorService>().AsSingle();
        Container.Bind<AgentStateBase>().To<AgentStateBaseCombatAgentIdleState>().AsSingle();
        Container.Bind<AgentStateBase>().To<AgentStateBaseCombatAgentAttackState>().AsSingle();
        Container.Bind<AgentStateBase>().To<AgentStateBaseCombatAgentDeadState>().AsSingle();
        Container.Bind<AgentStateMachine>().AsSingle();

        InstallFeatureBindings();
    }

    protected virtual void InstallFeatureBindings()
    {
    }
}
