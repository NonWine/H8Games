using Zenject;

public class SoldierCombatAgentInstaller : CombatAgentInstaller
{
    protected override void InstallFeatureBindings()
    {
        Container.Bind<BaseCombatAgentController>().To<SoldierCombatAgentController>().AsSingle().NonLazy();
    }
}
