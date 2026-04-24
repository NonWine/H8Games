using UnityEngine;
using Zenject;

public class  CombatAgentInstaller : MonoInstaller
{
    [SerializeField] private BaseCombatAgentView combatView;

    public override void InstallBindings()
    {
        Container.Bind<BaseCombatUnitView>().FromInstance(combatView).AsSingle();
        Container.Bind<BaseCombatAgentView>().FromInstance(combatView).AsSingle();

        InstallFeatureBindings();
    }

    protected virtual void InstallFeatureBindings()
    {
    }
}
