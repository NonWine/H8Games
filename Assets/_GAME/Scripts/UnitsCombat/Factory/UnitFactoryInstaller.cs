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

        Container.BindInterfacesAndSelfTo<CombatUnitFactory>().AsSingle().NonLazy();
            
    }
}

 