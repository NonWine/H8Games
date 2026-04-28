using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SoldierFactory
{
    private readonly DiContainer container;
    private readonly Dictionary<string, SoldierCombatUnitPool> pools;
    private readonly Dictionary<SoldierCombatAgentController, SoldierPoolableRoot> activeRoots = new();

    public SoldierFactory(DiContainer container, List<UnitCombatDefinition> definitions)
    {
        this.container = container;

        pools = new Dictionary<string, SoldierCombatUnitPool>(definitions.Count);

        foreach (UnitCombatDefinition definition in definitions)
        {
            SoldierCombatUnitPool pool = container.ResolveId<SoldierCombatUnitPool>(definition.UnitID);
            pools[definition.UnitID] = pool;
        }
    }

    public SoldierCombatAgentController Create(string unitId, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(unitId, out SoldierCombatUnitPool pool))
        {
            Debug.LogError($"[SoldierFactory] No pool found for unit '{unitId}'");
            return null;
        }
        Debug.Log(position);
        AgentSpawnParams spawnParams = new AgentSpawnParams(position, rotation, unitId);
        SoldierPoolableRoot root = pool.Spawn(spawnParams);
        SoldierCombatAgentController controller = root.Controller;

        activeRoots[controller] = root;
        controller.Died += () => activeRoots.Remove(controller);

        return controller;
    }

    public void Release(SoldierCombatAgentController soldier)
    {
        if (!activeRoots.TryGetValue(soldier, out SoldierPoolableRoot root))
        {
            return;
        }

        activeRoots.Remove(soldier);
        root.RequestDespawn();
    }
}
