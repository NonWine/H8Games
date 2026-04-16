using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UnitFactoryInstaller : MonoInstaller
{
    [SerializeField] private List<UnitCombatDefinition> unitCombatDefinitions;

    public override void InstallBindings()
    {

        Container.BindInstance(unitCombatDefinitions)
            .WhenInjectedInto<CombatUnitFactory>();

        Container.Bind<IFactory<string, BaseTargetingCombatAgent>>()
            .To<CombatUnitFactory>()
            .AsSingle();
            
    }
}

 