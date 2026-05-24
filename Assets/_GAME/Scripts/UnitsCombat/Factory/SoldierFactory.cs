using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SoldierFactory
{
    private readonly Dictionary<string, SoldierCombatUnitPool> pools;
    private readonly Dictionary<SoldierCombatAgentController, SoldierPoolableRoot> activeRoots = new();
    private readonly Dictionary<SoldierCombatAgentController, Action> diedHandlers = new();

    public SoldierFactory(DiContainer container, List<UnitCombatDefinition> definitions)
    {
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
        AgentSpawnParams spawnParams = new AgentSpawnParams(position, rotation, unitId);
        SoldierPoolableRoot root = pool.Spawn(spawnParams);

        SoldierCombatAgentController controller = root.Controller;
        activeRoots[controller] = root;

        // Named handler so we can unsubscribe on Release; prevents handler accumulation
        // across spawn/despawn cycles for the same pooled controller.
        Action diedHandler = null;
        diedHandler = () =>
        {
            controller.Died -= diedHandler;
            activeRoots.Remove(controller);
            diedHandlers.Remove(controller);
        };
        diedHandlers[controller] = diedHandler;
        controller.Died += diedHandler;

        return controller;
    }

    public void Release(SoldierCombatAgentController soldier)
    {
        if (!activeRoots.TryGetValue(soldier, out SoldierPoolableRoot root))
        {
            return;
        }

        if (diedHandlers.TryGetValue(soldier, out Action handler))
        {
            soldier.Died -= handler;
            diedHandlers.Remove(soldier);
        }

        activeRoots.Remove(soldier);
        root.RequestDespawn();
    }
}
