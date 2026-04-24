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
        Container.Bind<BaseCombatAgentController>().To<SoldierCombatAgentController>().AsSingle().NonLazy();
    }
}
