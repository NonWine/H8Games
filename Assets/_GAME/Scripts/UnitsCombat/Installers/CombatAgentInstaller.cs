using UnityEngine;
using Zenject;

public class CombatAgentInstaller : MonoInstaller
{
    [SerializeField] private BaseCombatAgentView view;

    public override void InstallBindings()
    {
        Container.Bind<BaseCombatUnitView>().FromInstance(view).AsSingle();
        Container.Bind<BaseCombatAgentView>().FromInstance(view).AsSingle();
    }
}
