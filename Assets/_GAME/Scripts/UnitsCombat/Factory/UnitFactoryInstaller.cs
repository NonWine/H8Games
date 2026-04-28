using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UnitFactoryInstaller : MonoInstaller
{
    [SerializeField] private List<UnitCombatDefinition> soldierDefinitions;

    public override void InstallBindings()
    {
        List<UnitCombatDefinition> validDefinitions = soldierDefinitions
            .Where(IsValid)
            .ToList();

        foreach (UnitCombatDefinition definition in validDefinitions)
        {
            Container.BindMemoryPool<SoldierPoolableRoot, SoldierCombatUnitPool>()
                .WithId(definition.UnitID)
                .WithInitialSize(definition.InitialPoolSize)
                .FromSubContainerResolve()
                .ByNewPrefab(definition.Prefab.gameObject)
                .UnderTransformGroup($"Soldiers [{definition.name}]");
        }

        Container.BindInstance(validDefinitions).WhenInjectedInto<SoldierFactory>();
        Container.Bind<SoldierFactory>().AsSingle();
    }

    private bool IsValid(UnitCombatDefinition definition)
    {
        if (definition.Prefab == null)
        {
            Debug.LogError($"[UnitFactoryInstaller] Definition '{definition.name}' has no prefab assigned");
            return false;
        }
        return true;
    }
}
