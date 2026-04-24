using UnityEngine;
using Zenject;

public class  CombatAgentInstaller : MonoInstaller
{
    [SerializeField] private BaseCombatAgentView combatView;
    protected BaseCombatAgentView CombatView => combatView;

    public override void InstallBindings()
    {
        Container.Bind<BaseCombatUnitView>().FromInstance(combatView).AsSingle();
        Container.Bind<BaseCombatAgentView>().FromInstance(combatView).AsSingle();
        Container.Bind<ICombatTargetValidator>().To<DefaultCombatTargetValidator>().AsSingle();

        InstallFeatureBindings();
    }

    protected virtual void InstallFeatureBindings()
    {
    }
}
