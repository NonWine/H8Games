using UnityEngine;
using Zenject;

public class PlayerCombatInstaller : MonoInstaller
{
    [SerializeField] private bool logAutoAttack;

    private BaseCombatUnitView combatView;

    public override void InstallBindings()
    {
        combatView = GetComponent<BaseCombatUnitView>();

        BindViews();
        BindData();
        BindTargeting();
        BindModules();
        BindHealthPresentation();
        BindStateMachine();
        BindControllers();
        ValidateAttackSetup();
    }

    private void BindViews()
    {
        Container.Bind<Transform>().FromInstance(combatView.transform).AsSingle();
        Container.Bind<BaseCombatUnitView>().FromInstance(combatView).AsSingle();
        Container.Bind<IAgentView>().FromInstance(combatView).AsSingle();
        Container.BindInstance(combatView.Animator).AsSingle();
    }

    private void BindData()
    {
        Container.Bind<UnitStats>()
            .FromMethod(context => context.Container.Resolve<HeroStats>().Combat)
            .AsSingle();
        
        Container.Bind<TargetingData>()
            .FromMethod(context => new TargetingData(context.Container.Resolve<HeroStats>().Targeting))
            .AsSingle();
    }

    private void BindTargeting()
    {
        Container.Bind<NearestEnemyCombatTargetProvider>().AsSingle();
        Container.Bind<ICombatTargetProvider>().To<HeroCombatTargetProvider>().AsSingle();
        Container.Bind<ICombatTargetValidator>().To<DefaultCombatTargetValidator>().AsSingle();
        Container.Bind<ICombatTargetValidator>().To<RangeCombatTargetValidator>().AsSingle();
        Container.Bind<ITargetReservationHandler>().To<TargetReservationHandler>().AsSingle();
        Container.Bind<ITargetTrackerHandler>().To<CombatTargetTracker>().AsSingle();
        Container.Bind<UnitRotatorService>().AsSingle();
        Container.Bind<HeroAnimationController>().AsSingle();

        Container.Bind<HeroAutoAttackLogger>().AsSingle().WithArguments(logAutoAttack);
    }

    private void BindModules()
    {
        Container.Bind<HeroRuntimeModel>().AsSingle();

        Container.Bind<IAliveState>()
            .FromMethod(context => context.Container.Resolve<HeroRuntimeModel>())
            .AsSingle();

        Container.Bind<IAliveStateReader>()
            .FromMethod(context => context.Container.Resolve<HeroRuntimeModel>())
            .AsSingle();
        Container.Bind<IHealthModule>()
            .FromMethod(context => new UnitHealthHandler(
                context.Container.Resolve<UnitStats>().MaxHealth,
                context.Container.Resolve<IAliveState>()))
            .AsSingle();

        Container.Bind<AttackRuntimeModel>().AsSingle();

        Container.Bind<ProjectileVisualSpawner>()
            .AsSingle()
            .WithArguments(combatView.ProjectilePrefab);

        Container.Bind<IAttackModule>().To<UnitAttackAgentHandler>().AsSingle();
    }

    // The bar itself lives on the prefab's nested HealthUI object; the hero side
    // only ever sees IHealthView, so swapping in a different bar is a change to
    // this one binding.
    private void BindHealthPresentation()
    {
        Container.Bind<IHealthView>()
            .FromMethod(context => context.Container.Resolve<PlayerView>().HealthBar)
            .AsSingle();

        Container.BindInterfacesAndSelfTo<HeroHealthPresenter>().AsSingle();
        Container.BindInterfacesAndSelfTo<HeroHealthResetService>().AsSingle();
    }

    private void BindStateMachine()
    {
        Container.Bind<HeroStateBase>().To<HeroIdleState>().AsSingle();
        Container.Bind<HeroStateBase>().To<HeroAttackState>().AsSingle();
        Container.Bind<HeroStateBase>().To<HeroDeadState>().AsSingle();
        Container.Bind<HeroStateMachine>().AsSingle();
    }

    private void BindControllers()
    {
        Container.BindInterfacesAndSelfTo<HeroCombatAgentController>().AsSingle();
        Container.BindInterfacesAndSelfTo<HeroFacingController>().AsSingle();
    }
    
    private void ValidateAttackSetup()
    {
        if (combatView.AttackPoint == null)
        {
            HeroAutoAttackLogger.LogMissingAttackOrigin();
        }

        if (combatView.ProjectilePrefab == null)
        {
            HeroAutoAttackLogger.LogMissingProjectilePrefab();
        }
    }
}
