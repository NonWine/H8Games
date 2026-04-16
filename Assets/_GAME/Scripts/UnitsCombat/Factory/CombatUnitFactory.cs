using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class CombatUnitFactory : IFactory<string, BaseTargetingCombatAgent>
{
    private readonly DiContainer _container;
    private readonly Dictionary<string, UnitCombatDefinition> _definitions;

    public CombatUnitFactory(DiContainer container, List<UnitCombatDefinition> definitions)
    {
        _container = container;
        _definitions = new Dictionary<string, UnitCombatDefinition>();

        foreach (var definition in definitions.Where(x => x != null))
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

            if (_definitions.ContainsKey(definition.UnitID))
            {
                Debug.LogError($"[CombatUnitFactory] Duplicate UnitID '{definition.UnitID}'");
                continue;
            }

            _definitions.Add(definition.UnitID, definition);
        }
    }

    public BaseTargetingCombatAgent Create(string id)
    {
        if (!_definitions.TryGetValue(id, out var definition))
        {
            Debug.LogError($"[CombatUnitFactory] Prefab for type '{id}' is missing in installer");
            return null;
        }

        var instance = _container.InstantiatePrefab(definition.Prefab.gameObject);

        var unit = instance.GetComponent<BaseTargetingCombatAgent>();
        if (unit == null)
        {
            unit = instance.GetComponentInChildren<BaseTargetingCombatAgent>();
        }

        if (unit == null)
        {
            Debug.LogError($"[CombatUnitFactory] Spawned prefab '{definition.name}' has no BaseTargetingCombatAgent");
            return null;
        }

        unit.SetIdentity(id);
        return unit;
    }
}