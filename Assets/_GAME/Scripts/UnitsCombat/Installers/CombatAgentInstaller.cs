using UnityEngine;
using Zenject;

public class CombatAgentInstaller : MonoInstaller
{
    [SerializeField] private BaseCombatAgentView combatView;
    protected BaseCombatAgentView CombatView => combatView;

    public override void InstallBindings()
    {
        BindCore();
        BindSharedServices();
        InstallFeatureBindings();
        BindCombatModules();
    }

    protected virtual void InstallFeatureBindings()
    {
    }

    private void BindCore()
    {
        UnitStats unitStats = combatView.unitConfig.CreateRuntimeStats();

        Container.Bind<BaseCombatUnitView>().FromInstance(combatView).AsSingle();
        Container.Bind<BaseCombatAgentView>().FromInstance(combatView).AsSingle();
        Container.BindInstance(combatView.Animator).AsSingle();
        Container.Bind<UnitStats>().FromInstance(unitStats).AsSingle();
        Container.Bind<AgentRuntimeModel>().AsSingle();
    }

    private void BindSharedServices()
    {
        Container.Bind<ICombatTargetValidator>().To<DefaultCombatTargetValidator>().AsSingle();
        Container.Bind<ITargetReservationHandler>().To<TargetReservationHandler>().AsSingle();
        Container.Bind<ITargetTrackerHandler>()
            .To<CombatTargetTracker>()
            .AsSingle()
            .WithArguments(
                combatView.unitConfig.AuthoringStats.RetargetInterval,
                combatView.unitConfig.AuthoringStats.TargetLockDuration);
        Container.Bind<UnitRotatorService>().AsSingle();
        Container.Bind<AgentAnimationController>().AsSingle();
    }

    private void BindCombatModules()
    {
        Container.Bind<CombatUnitModules>()
            .FromMethod(context =>
            {
                ModulesFactoryCollection modulesFactoryCollection = context.Container.Resolve<ModulesFactoryCollection>();
                IUnitModulesFactory unitModuleFactory = modulesFactoryCollection.Create(combatView.unitConfig.unitModuleType);
                AgentRuntimeModel runtimeModel = context.Container.Resolve<AgentRuntimeModel>();
                UnitStats unitStats = context.Container.Resolve<UnitStats>();
                return unitModuleFactory.Create(new CombatUnitModulesArgs(combatView, unitStats, runtimeModel));
            })
            .AsSingle();
    }
}
