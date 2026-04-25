using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class CombatUnitFactory : IFactory<string, IAgentController>
{
    private readonly DiContainer container;
    private readonly Dictionary<string, UnitCombatDefinition> definitions;

    public CombatUnitFactory(DiContainer container, List<UnitCombatDefinition> definitions)
    {
        this.container = container;
        this.definitions = new Dictionary<string, UnitCombatDefinition>();

        foreach (UnitCombatDefinition definition in definitions.Where(x => x != null))
        {
            if (string.IsNullOrWhiteSpace(definition.UnitID))
            {
                Debug.LogError($"[CombatUnitFactory] Definition '{definition.name}' has empty UnitID");
                continue;
            }

            if (definition.Prefab == null)
            {
                Debug.LogError($"[CombatUnitFactory] Definition '{definition.name}' has no prefab assigned");
                continue;
            }

            if (this.definitions.ContainsKey(definition.UnitID))
            {
                Debug.LogError($"[CombatUnitFactory] Duplicate UnitID '{definition.UnitID}'");
                continue;
            }

            this.definitions.Add(definition.UnitID, definition);
        }
    }

    public IAgentController Create(string id)
    {
        if (!definitions.TryGetValue(id, out UnitCombatDefinition definition))
        {
            Debug.LogError($"[CombatUnitFactory] Prefab for type '{id}' is missing in installer");
            return null;
        }

        GameObject instance = container.InstantiatePrefab(definition.Prefab.gameObject);
        GameObjectContext context = instance.GetComponent<GameObjectContext>();

        if (context == null)
        {
            Debug.LogError($"[CombatUnitFactory] Spawned prefab '{definition.name}' has no GameObjectContext");
            Object.Destroy(instance);
            return null;
        }

        IAgentController controller;

        try
        {
            controller = context.Container.Resolve<IAgentController>();
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[CombatUnitFactory] Spawned prefab '{definition.name}' could not resolve BaseCombatAgentController: {exception.Message}");
            Object.Destroy(instance);
            return null;
        }

        controller.SetIdentity(id);
        return controller;
    }
}
