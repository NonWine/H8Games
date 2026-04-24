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
        Container.Bind<BaseCombatAgentController>().To<EnemyCombatAgentController>().AsSingle().NonLazy();
    }
}
