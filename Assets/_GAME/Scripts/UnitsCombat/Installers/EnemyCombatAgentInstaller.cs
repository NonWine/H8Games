using Zenject;

public class EnemyCombatAgentInstaller : CombatAgentInstaller
{
    protected override void InstallFeatureBindings()
    {
        Container.Bind<BaseCombatAgentController>().To<EnemyCombatAgentController>().AsSingle().NonLazy();
    }
}
