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
        BindModules();
        InstallFeatureBindings();
    }

    protected virtual void InstallFeatureBindings()
    {
    }

    protected virtual void BindModules()
    {
        Container.Bind<AttackRuntimeModel>().AsSingle();

        Container.Bind<ProjectileVisualSpawner>()
            .AsSingle()
            .WithArguments(CombatView.ProjectilePrefab);

        Container.Bind<IAttackModule>().To<UnitAttackAgentHandler>().AsSingle();

        Container.Bind<IHealthModule>().To<UnitHealthHandler>()
            .AsSingle()
            .WithArguments(CombatView.unitConfig.AuthoringStats.MaxHealth);

        Container.Bind<IDeathModule>().To<UnitDeathModule>()
            .AsSingle()
            .WithArguments(CombatView.gameObject);

        Container.Bind<CombatUnitModules>().AsSingle();
    }

    private void BindCore()
    {
        UnitStats unitStats = combatView.unitConfig.CreateRuntimeStats();
        TargetingData targetingData = combatView.unitConfig.CreateTargetingData();
        Container.Bind<Transform>().FromInstance(combatView.transform).AsSingle();
        Container.Bind<BaseCombatUnitView>().FromInstance(combatView).AsSingle();
        Container.Bind<BaseCombatAgentView>().FromInstance(combatView).AsSingle();
        Container.BindInstance(combatView.Animator).AsSingle();
        Container.Bind<UnitStats>().FromInstance(unitStats).AsSingle();
        Container.Bind<TargetingData>().FromInstance(targetingData).AsSingle();

    }

    private void BindSharedServices()
    {
        Container.Bind<ICombatTargetValidator>().To<DefaultCombatTargetValidator>().AsSingle();
        Container.Bind<ITargetReservationHandler>().To<TargetReservationHandler>().AsSingle();
        Container.Bind<ITargetTrackerHandler>().To<CombatTargetTracker>().AsSingle();
        Container.Bind<UnitRotatorService>().AsSingle();
        Container.Bind<AgentAnimationController>().AsSingle();
    }
}
